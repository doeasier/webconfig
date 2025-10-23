using Microsoft.Extensions.Configuration.WebConfig;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration
{
    public class WebConfigurationProvider : IConfigurationProvider
    {
        private static readonly HttpClient s_httpClient = new HttpClient();
        private readonly WebConfigurationSource _configurationSource;
        private readonly object _dataLock = new();
        private IDictionary<string, string?> _data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        private CancellationTokenSource _reloadToken = new();
        private IDisposable? _changeRegistration;
        private string? _lastEtag;
        private DateTimeOffset? _lastModified;

        // write-back queue
        private readonly object _wbLock = new();
        private readonly Dictionary<string, string?> _pending = new(StringComparer.OrdinalIgnoreCase);
        private System.Timers.Timer? _batchTimer;
        private bool _suppressWrite = false;

        public WebConfigurationProvider(WebConfigurationSource source)
        {
            ArgumentNullException.ThrowIfNull(source);
            _configurationSource = source;
            if (!string.IsNullOrWhiteSpace(_configurationSource.AccessToken))
            {
                s_httpClient.DefaultRequestHeaders.Remove("Access-Token");
                s_httpClient.DefaultRequestHeaders.Add("Access-Token", _configurationSource.AccessToken);
            }
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        {
            var result = new List<string>();
            if (earlierKeys != null) result.AddRange(earlierKeys);
            var prefix = parentPath is null ? null : parentPath + ConfigurationPath.KeyDelimiter;
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            lock (_dataLock)
            {
                foreach (var fullKey in _data.Keys)
                {
                    if (prefix == null)
                    {
                        var idx = fullKey.IndexOf(ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
                        var segment = idx >= 0 ? fullKey[..idx] : fullKey;
                        if (!string.IsNullOrEmpty(segment)) keys.Add(segment);
                    }
                    else if (fullKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        var remainder = fullKey[prefix.Length..];
                        var idx = remainder.IndexOf(ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
                        var segment = idx >= 0 ? remainder[..idx] : remainder;
                        if (!string.IsNullOrEmpty(segment)) keys.Add(segment);
                    }
                }
            }
            result.AddRange(keys);
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public IChangeToken GetReloadToken() => new CancellationChangeToken(_reloadToken.Token);

        public void Load()
        {
            try
            {
                _configurationSource.EnsureDefaults(new ConfigurationBuilder());
                var url = _configurationSource.Url ?? _configurationSource.WebProvider?.GetConfingInfo().Url;
                if (string.IsNullOrWhiteSpace(url))
                {
                    if (_configurationSource.Optional)
                    { ReplaceData(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)); return; }
                    throw new InvalidOperationException("未指定有效的 Url。");
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_lastEtag)) request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(_lastEtag));
                if (_lastModified.HasValue) request.Headers.IfModifiedSince = _lastModified;

                using var response = s_httpClient.Send(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    EnsureChangeListener();
                    return;
                }
                response.EnsureSuccessStatusCode();

                _lastEtag = response.Headers.ETag?.Tag;
                _lastModified = response.Content.Headers.LastModified;

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var parsed = ParseJsonToFlatDictionary(json);
                // Prevent write-back on ReplaceData
                _suppressWrite = true;
                ReplaceData(parsed);
                _suppressWrite = false;
                EnsureChangeListener();
            }
            catch (Exception ex)
            {
                var ctx = new WebLoadExceptionContext
                {
                    Provider = this,
                    Exception = ex,
                    Ignore = _configurationSource.Optional
                };
                _configurationSource.OnLoadException?.Invoke(ctx);
                if (!ctx.Ignore) throw;
            }
        }

        public void Set(string key, string? value)
        {
            ArgumentNullException.ThrowIfNull(key);
            lock (_dataLock) {
                _data[key] = value; 
            }
            SignalReload();

            if (_suppressWrite || !_configurationSource.EnableWriteBack) return;
            EnqueueWrite(key, value);
        }

        private void EnqueueWrite(string key, string? value)
        {
            lock (_wbLock)
            {
                _pending[key] = value;
                if (!_configurationSource.BatchWrites)
                {
                    _ = Task.Run(() => FlushPendingAsync());
                    return;
                }
                _batchTimer ??= new System.Timers.Timer(Math.Max(100, _configurationSource.BatchWindowSeconds * 1000)) { AutoReset = false };
                _batchTimer.Stop();
                _batchTimer.Elapsed += async (_, __) => await FlushPendingAsync();
                _batchTimer.Start();
            }
        }

        private async Task FlushPendingAsync()
        {
            KeyValuePair<string, string?>[] items;
            lock (_wbLock)
            {
                items = _pending.ToArray();
                _pending.Clear();
                _batchTimer?.Stop();
            }
            if (items.Length == 0) return;

            try
            {
                await SendWriteAsync(items);
            }
            catch (HttpRequestException)
            {
                // network error: requeue
                lock (_wbLock)
                {
                    foreach (var kv in items) _pending[kv.Key] = kv.Value;
                }
            }
        }

        private async Task SendWriteAsync(KeyValuePair<string, string?>[] items)
        {
            var url = _configurationSource.Url;
            if (string.IsNullOrWhiteSpace(url)) return;
            using var req = new HttpRequestMessage(items.Length == 1 ? HttpMethod.Post : HttpMethod.Put, url);
            if (_configurationSource.UseOptimisticConcurrency && !string.IsNullOrEmpty(_lastEtag))
            {
                req.Headers.TryAddWithoutValidation("If-Match", _lastEtag);
            }
            if (items.Length == 1)
            {
                var body = new { key = items[0].Key, value = items[0].Value };
                req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            }
            else
            {
                var body = new { items = items.Select(kv => new { key = kv.Key, value = kv.Value }).ToArray() };
                req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            }

            using var resp = await s_httpClient.SendAsync(req).ConfigureAwait(false);
            if (resp.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
            {
                // 412: reload latest, merge (local wins), retry once
                Load();
                // merge local pending data over loaded data
                lock (_dataLock)
                {
                    foreach (var kv in items) _data[kv.Key] = kv.Value;
                }
                SignalReload();
                // retry once
                using var retryReq = new HttpRequestMessage(items.Length == 1 ? HttpMethod.Post : HttpMethod.Put, url);
                if (_configurationSource.UseOptimisticConcurrency && !string.IsNullOrEmpty(_lastEtag))
                    retryReq.Headers.TryAddWithoutValidation("If-Match", _lastEtag);
                retryReq.Content = req.Content;
                using var retryResp = await s_httpClient.SendAsync(retryReq).ConfigureAwait(false);
                retryResp.EnsureSuccessStatusCode();
                _lastEtag = retryResp.Headers.ETag?.Tag ?? _lastEtag;
                _lastModified = retryResp.Content?.Headers?.LastModified ?? _lastModified;
                return;
            }
            resp.EnsureSuccessStatusCode();
            _lastEtag = resp.Headers.ETag?.Tag ?? _lastEtag;
            _lastModified = resp.Content?.Headers?.LastModified ?? _lastModified;
        }

        public bool TryGet(string key, out string? value)
        {
            ArgumentNullException.ThrowIfNull(key);
            lock (_dataLock) { return _data.TryGetValue(key, out value); }
        }

        private void ReplaceData(IDictionary<string, string?> newData)
        {
            lock (_dataLock) { _data = newData; }
            SignalReload();
        }

        private void SignalReload()
        {
            var prev = Interlocked.Exchange(ref _reloadToken, new CancellationTokenSource());
            try { prev.Cancel(); } catch { }
        }

        private void EnsureChangeListener()
        {
            if (!_configurationSource.ReloadOnChange) return;
            if (_configurationSource.WebProvider is null) return;
            if (_changeRegistration != null) return;

            _changeRegistration = ChangeToken.OnChange(
                () => _configurationSource.WebProvider.Watch(_configurationSource.ReloadDelay, _configurationSource.AccessToken),
                () =>
                {
                    var delay = Math.Max(0, _configurationSource.ReloadDelay) * 1000;
                    Task.Run(async () =>
                    {
                        try
                        {
                            if (delay > 0) await Task.Delay(delay).ConfigureAwait(false);
                            Load();
                        }
                        catch (Exception ex)
                        {
                            var ctx = new WebLoadExceptionContext
                            {
                                Provider = this,
                                Exception = ex,
                                Ignore = _configurationSource.Optional
                            };
                            _configurationSource.OnLoadException?.Invoke(ctx);
                            if (!ctx.Ignore) throw;
                        }
                    });
                });
        }

        private static Dictionary<string, string?> ParseJsonToFlatDictionary(string json)
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new FormatException("配置根必须是 JSON 对象。");

            void Visit(JsonElement el, string? parent)
            {
                string Combine(string name) => string.IsNullOrEmpty(parent) ? name : parent + ConfigurationPath.KeyDelimiter + name;
                switch (el.ValueKind)
                {
                    case JsonValueKind.Object:
                        foreach (var p in el.EnumerateObject()) Visit(p.Value, Combine(p.Name));
                        break;
                    case JsonValueKind.Array:
                        var i = 0; foreach (var item in el.EnumerateArray()) { Visit(item, Combine(i.ToString(CultureInfo.InvariantCulture))); i++; }
                        break;
                    case JsonValueKind.String: if (parent != null) result[parent] = el.GetString(); break;
                    case JsonValueKind.Number: if (parent != null) result[parent] = el.GetRawText(); break;
                    case JsonValueKind.True:
                    case JsonValueKind.False: if (parent != null) result[parent] = el.GetBoolean().ToString(CultureInfo.InvariantCulture); break;
                    case JsonValueKind.Null: if (parent != null) result[parent] = null; break;
                    default: if (parent != null) result[parent] = el.GetRawText(); break;
                }
            }
            Visit(doc.RootElement, null);
            return result;
        }
    }
}
