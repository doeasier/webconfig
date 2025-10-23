using Microsoft.Extensions.Configuration.WebConfig;

namespace Microsoft.Extensions.Configuration;

public static class WebProviderConfigurationExtensions
{
    private const string WebProviderKey = "WebProvider";

    private const string WebLoadExceptionHandlerKey = "WebLoadExceptionHandler";

    //
    // 摘要:
    //     Sets the default Microsoft.Extensions.Configuration.WebProviders.IWebProvider to be used
    //     for web-based providers.
    //
    // 参数:
    //   builder:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.
    //
    //   webProvider:
    //     The default web provider instance.
    //
    // 返回结果:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder.
    public static IConfigurationBuilder SetWebProvider(this IConfigurationBuilder builder, IWebProvider webProvider)
    {
        ArgumentNullException.ThrowIfNull(builder, "builder");
        ArgumentNullException.ThrowIfNull(webProvider, "webProvider");
        builder.Properties[WebProviderKey] = webProvider;
        return builder;
    }

    //
    // 摘要:
    //     Gets the default Microsoft.Extensions.Configuration.WebConfig.IWebProvider to be used
    //     for web-based providers.
    //
    // 参数:
    //   builder:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder.
    //
    // 返回结果:
    //     The default Microsoft.Extensions.Configuration.WebConfig.IWebProvider.
    public static IWebProvider GetWebProvider(this IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, "builder");
        if (builder.Properties.TryGetValue(WebProviderKey, out object? value))
        {
            return (IWebProvider)value;
        }

        return new WebProvider();
    }

    //
    // 摘要:
    //     Sets the WebProvider for web-based providers to a WebProvider with
    //     the base path.
    //
    // 参数:
    //   builder:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.
    //
    //   basePath:
    //     The absolute url of web-based providers. 
    //
    // 返回结果:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder.
    public static IConfigurationBuilder SetWebUrl(this IConfigurationBuilder builder, string Url)
    {
        ArgumentNullException.ThrowIfNull(builder, "builder");
        ArgumentNullException.ThrowIfNull(Url, "url");
        return builder.SetWebProvider(new WebProvider(Url));
    }

    //
    // 摘要:
    //     Sets a default action to be invoked for web-based providers when an error occurs.
    //
    //
    // 参数:
    //   builder:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.
    //
    //   handler:
    //     The Action to be invoked on a web load exception.
    //
    // 返回结果:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder.
    public static IConfigurationBuilder SetWebLoadExceptionHandler(this IConfigurationBuilder builder, Action<WebLoadExceptionContext> handler)
    {
        ArgumentNullException.ThrowIfNull(builder, "builder");
        builder.Properties[WebLoadExceptionHandlerKey] = handler;
        return builder;
    }

    //
    // 摘要:
    //     Gets a default action to be invoked for web-based providers when an error occurs.
    //
    //
    // 参数:
    //   builder:
    //     The Microsoft.Extensions.Configuration.IConfigurationBuilder.
    //
    // 返回结果:
    //     The The Action to be invoked on a web load exception, if set.
    public static Action<WebLoadExceptionContext>? GetWebLoadExceptionHandler(this IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, "builder");
        if (builder.Properties.TryGetValue(WebLoadExceptionHandlerKey, out object? value))
        {
            return value as Action<WebLoadExceptionContext>;
        }

        return null;
    }
}

