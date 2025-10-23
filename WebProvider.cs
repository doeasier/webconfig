using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.WebConfig
{
    public class WebProvider : IWebProvider
    {
        private static readonly HttpClient s_httpClient = new HttpClient();
        private static bool s_tokenSet = false;
        private readonly WebConfigInfo _webConfigInfo;

        public WebProvider()
        {
            _webConfigInfo = new WebConfigInfo();
        }

        public WebProvider(string Url)
        {
            _webConfigInfo = new WebConfigInfo(Url);
        }

        public WebConfigInfo GetConfingInfo()
        {
            return _webConfigInfo;
        }

        public IChangeToken Watch(int reloadDelay, string? accessToken)
        {
            // 未配置 URL 时不提供变更通知
            if (string.IsNullOrWhiteSpace(_webConfigInfo.Url))
            {
                return NullChangeToken.Singleton;
            }

            var cts = new CancellationTokenSource();
            var pollInterval = TimeSpan.FromSeconds(reloadDelay >= 30 && reloadDelay <= 36000 ? reloadDelay:240);

            var url = _webConfigInfo.Url;
            var baseline = _webConfigInfo.LastModified.ToUniversalTime();

            _ = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var serverLast = await GetServerLastModifiedAsync(url!, accessToken, cts.Token).ConfigureAwait(false);
                        if (serverLast.HasValue)
                        {
                            var serverUtc = serverLast.Value.UtcDateTime;
                            // 发现服务端更新时间更晚则触发变更
                            if (serverUtc > baseline)
                            {
                                _webConfigInfo.LastModified = serverUtc;
                                try { cts.Cancel(); } catch { }
                                break;
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // 网络/解析异常忽略，继续轮询
                    }

                    try
                    {
                        await Task.Delay(pollInterval, cts.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, CancellationToken.None);

            return new CancellationChangeToken(cts.Token);
        }

        private static async Task<DateTimeOffset?> GetServerLastModifiedAsync(string url, string? accessToken, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(accessToken)&&!s_tokenSet)
            {
                bool found = false;
                s_tokenSet = true;
                foreach (var header in s_httpClient.DefaultRequestHeaders)
                {
                    if (header.Key.Equals("Access-Token"))
                    { 
                        found = true; break;
                    }
                }
                if (!found)
                {
                    s_httpClient.DefaultRequestHeaders.Add("Access-Token", accessToken!);
                }
            }
            // 优先使用 HEAD，若失败则回退到 GET（只取响应头）
            try
            {
                using var head = new HttpRequestMessage(HttpMethod.Head, url);
                using var resp = await s_httpClient.SendAsync(head, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                if (resp.Content?.Headers?.LastModified is DateTimeOffset lm1)
                {
                    return lm1;
                }
                if (resp.Headers.TryGetValues("Last-Modified", out var vals1) &&
                    DateTimeOffset.TryParse(vals1.FirstOrDefault(), out var lmParsed1))
                {
                    return lmParsed1;
                }
            }
            catch (TaskCanceledException) { throw; }
            catch { /* ignore and fallback */ }

            try
            {
                using var get = new HttpRequestMessage(HttpMethod.Get, url);
                using var resp = await s_httpClient.SendAsync(get, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                if (resp.Content?.Headers?.LastModified is DateTimeOffset lm2)
                {
                    return lm2;
                }
                if (resp.Headers.TryGetValues("Last-Modified", out var vals2) &&
                    DateTimeOffset.TryParse(vals2.FirstOrDefault(), out var lmParsed2))
                {
                    return lmParsed2;
                }
            }
            catch (TaskCanceledException) { throw; }
            catch { /* ignore */ }

            return null;
        }
    }
}