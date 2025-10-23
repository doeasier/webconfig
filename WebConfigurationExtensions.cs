using Microsoft.Extensions.Configuration.WebConfig;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for adding <see cref="WebConfigurationProvider"/>.
    /// </summary>
    public static class WebConfigurationExtensions
    {
        /// <summary>
        /// Adds the WEB configuration provider at <paramref name="URL"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, string URL, string accessToken)
        {
            return AddWebConfig(builder, URL, accessToken, optional: false);
        }


        /// <summary>
        /// Adds the WEB configuration provider at <paramref name="URL"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="optional">Whether the web api is optional.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, string URL, string accessToken, bool optional)
        {
            return AddWebConfig(builder, URL, accessToken, optional, reloadOnChange: false);
        }

        /// <summary>
        /// Adds the WEB configuration provider at <paramref name="URL"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="optional">Whether the web api is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the config changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, 
            string URL, 
            string accessToken, 
            bool optional, 
            bool reloadOnChange)
        {
            return AddWebConfig(builder, URL, accessToken, optional, reloadOnChange, 240);
        }

        /// <summary>
        /// Adds a WEB configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="AccessToken">The access token.</param>
        /// <param name="optional">Whether the URL is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the URL changes.</param>
        /// <param name="checkInterval">The interval for check config changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder,
            string URL,
            string? accessToken,
            bool optional,
            bool reloadOnChange,
            int checkInterval
            )
        {
            return AddWebConfig(builder, null, URL, accessToken, optional, reloadOnChange, checkInterval, false, true, 30);
        }

        /// <summary>
        /// Adds a WEB configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="AccessToken">The access token.</param>
        /// <param name="optional">Whether the URL is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the URL changes.</param>
        /// <param name="enableWriteBack">Save new value to server.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder,
            string URL,
            string? AccessToken,
            bool optional,
            bool reloadOnChange,
            bool enableWriteBack
            ) => AddWebConfig(
                builder,
                URL,
                AccessToken,
                optional,
                reloadOnChange,
                60,
                enableWriteBack);

        /// <summary>
        /// Adds a WEB configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="AccessToken">The access token.</param>
        /// <param name="optional">Whether the URL is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the URL changes.</param>
        /// <param name="checkInterval">The interval for check config changes.</param>
        /// <param name="enableWriteBack">Save new value to server.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, 
            string URL, 
            string? accessToken, 
            bool optional, 
            bool reloadOnChange,
            int checkInterval,
            bool enableWriteBack) => AddWebConfig(
                builder, 
                null, 
                URL, 
                accessToken, 
                optional, 
                reloadOnChange, 
                checkInterval,
                enableWriteBack,
                true,
                3);

        /// <summary>
        /// Adds a WEB configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="AccessToken">The access token.</param>
        /// <param name="optional">Whether the URL is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the URL changes.</param>
        /// <param name="checkInterval">The interval for check config changes.</param>
        /// <param name="enableWriteBack">Save new value to server.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder,
            string URL,
            string? accessToken,
            bool optional,
            bool reloadOnChange,
            int checkInterval,
            bool enableWriteBack,
            bool batchWrites,
            int batchWindowSeconds
            ) => AddWebConfig(
                builder,
                null,
                URL,
                accessToken,
                optional,
                reloadOnChange,
                checkInterval,
                enableWriteBack,
                batchWrites,
                batchWindowSeconds);

        /// <summary>
        /// Adds a WEB configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="provider">The <see cref="IWebProvider"/> to use to access the web api.</param>
        /// <param name="URL">Url relative to the web api root path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="AccessToken">The access token.</param>
        /// <param name="optional">Whether the URL is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the URL changes.</param>
        /// <param name="enableWriteBack">Whether the configuration should be write back to server.</param>
        /// <param name="batchWrites">Whether the configuration should be write back to server in batch.</param>
        /// <param name="batchWindowSeconds">The batch window seconds.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, 
            IWebProvider? provider, 
            string URL, 
            string? accessToken, 
            bool optional, 
            bool reloadOnChange,
            int checkInterval,
            bool enableWriteBack,
            bool batchWrites,
            int batchWindowSeconds)
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (string.IsNullOrWhiteSpace(URL))
            {
                // 使用明确的消息文本，避免对内部资源 SR 的依赖，并确保参数类型匹配
                throw new ArgumentException("URL 不能为空或无效。", paramName: nameof(URL));
            }

            //固定接口api
            if (URL.EndsWith("/")) URL += "api/config";
            else URL += "/api/config";

            return builder.AddWebConfig(s =>
            {
                s.WebProvider = provider;
                s.Url = URL;
                s.AccessToken = accessToken;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.ReloadDelay = checkInterval;
                s.EnableWriteBack = enableWriteBack;
                s.BatchWrites = batchWrites;
                s.BatchWindowSeconds = batchWindowSeconds;
                s.ResolveWebProvider();
            });
        }
        /// <summary>
        /// Adds a WEB configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, Action<WebConfigurationSource>? configureSource)
            => builder.Add(configureSource);
    }
}
