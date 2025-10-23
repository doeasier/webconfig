using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.WebConfig
{
    public class WebConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Used to access the contents of the web api.
        /// </summary>
        public IWebProvider? WebProvider { get; set; }

        /// <summary>
        /// The url to the api.
        /// </summary>
        [DisallowNull]
        public string? Url { get; set; }

        /// <summary>
        /// The access token to use when accessing the api.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Enable write-back via POST/PUT /api/config.
        /// </summary>
        public bool EnableWriteBack { get; set; } = false;

        /// <summary>
        /// Batch write multiple Set calls within the window.
        /// </summary>
        public bool BatchWrites { get; set; } = true;

        /// <summary>
        /// Batch window in seconds.
        /// </summary>
        public int BatchWindowSeconds { get; set; } = 3;

        /// <summary>
        /// Use optimistic concurrency (If-Match ETag/If-Unmodified-Since)
        /// </summary>
        public bool UseOptimisticConcurrency { get; set; } = true;

        /// <summary>
        /// The data is encrypted.  The default is false.
        /// </summary>
        public bool Enprypted { get; set; } = false;

        /// <summary>
        /// Determines if loading the web api is optional.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Determines whether the source will be loaded if the underlying api changes.
        /// </summary>
        public bool ReloadOnChange { get; set; }

        /// <summary>
        /// Number of seconds that reload will wait before calling Load.  This helps
        /// avoid triggering reload before a api is completely written. Default is 240.
        /// </summary>
        public int ReloadDelay { get; set; } = 240;

        /// <summary>
        /// Will be called if an uncaught exception occurs in WebConfigurationProvider.Load.
        /// </summary>
        public Action<WebLoadExceptionContext>? OnLoadException { get; set; }

        /// <summary>
        /// Builds the <see cref="IConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="IConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (string.IsNullOrWhiteSpace(Url))
            {
                throw new ArgumentException("Url 不能为空或无效。", nameof(Url));
            }

            // 应用默认值（WebProvider、OnLoadException）
            EnsureDefaults(builder);

            // 若仍未提供 WebProvider，尝试按 Url 解析
            if (WebProvider is null)
            {
                ResolveWebProvider();
            }

            if (WebProvider is null && !Optional)
            {
                throw new InvalidOperationException("未能解析 WebProvider，且该源非可选。");
            }

            // 交给 Provider 完成具体的拉取与监听逻辑（需要 WebConfigurationProvider(WebConfigurationSource) 构造函数）
            return new WebConfigurationProvider(this);
        }

        /// <summary>
        /// Called to use any default settings on the builder like the WebProvider or WebLoadExceptionHandler.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        public void EnsureDefaults(IConfigurationBuilder builder)
        {
            WebProvider ??= builder.GetWebProvider();
            OnLoadException ??= builder.GetWebLoadExceptionHandler();
        }

        /// <summary>
        /// If no web provider has been set, for absolute web, this will creates a web provider
        /// </summary>
        public void ResolveWebProvider()
        {
            if (WebProvider == null &&
                !string.IsNullOrEmpty(Url))
            {
                var is_exists = false;
                try { 
                    var uri = new Uri(Url);
                    using var client = new HttpClient();
                    if(!string.IsNullOrEmpty(AccessToken)) client.DefaultRequestHeaders.Add("Access-Token", AccessToken);
                    var response = client.GetAsync(uri).Result;
                    is_exists = response.IsSuccessStatusCode;
                    if (is_exists)
                    {
                        WebProvider = new WebProvider(Url);
                    }
                }
                catch {
                    WebProvider = null;
                }
            }
        }
    }
}
