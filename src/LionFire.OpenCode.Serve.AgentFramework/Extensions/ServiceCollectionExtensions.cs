using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LionFire.OpenCode.Serve.AgentFramework;

/// <summary>
/// Extension methods for registering Agent Framework services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="IChatClient"/> implementation using OpenCode.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional delegate to configure the chat client options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddOpenCodeClient(options =>
    /// {
    ///     options.BaseUrl = "http://localhost:9123";
    /// })
    /// .Services.AddOpenCodeChatClient(chatOptions =>
    /// {
    ///     chatOptions.ModelId = "opencode-claude";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenCodeChatClient(
        this IServiceCollection services,
        Action<OpenCodeChatClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.TryAddSingleton<IChatClient>(sp =>
        {
            var openCodeClient = sp.GetRequiredService<IOpenCodeClient>();
            var options = sp.GetService<Microsoft.Extensions.Options.IOptions<OpenCodeChatClientOptions>>();
            return new OpenCodeChatClient(openCodeClient, options?.Value);
        });

        return services;
    }

    /// <summary>
    /// Adds a keyed <see cref="IChatClient"/> implementation using OpenCode.
    /// Useful when you have multiple chat clients registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="key">The key to use for the service.</param>
    /// <param name="configureOptions">Optional delegate to configure the chat client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKeyedOpenCodeChatClient(
        this IServiceCollection services,
        string key,
        Action<OpenCodeChatClientOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var options = new OpenCodeChatClientOptions();
        configureOptions?.Invoke(options);

        services.AddKeyedSingleton<IChatClient>(key, (sp, _) =>
        {
            var openCodeClient = sp.GetRequiredService<IOpenCodeClient>();
            return new OpenCodeChatClient(openCodeClient, options);
        });

        return services;
    }
}
