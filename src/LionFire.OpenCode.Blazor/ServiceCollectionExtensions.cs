using LionFire.OpenCode.Blazor.Services;
using LionFire.OpenCode.Blazor.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.OpenCode.Blazor;

/// <summary>
/// Extension methods for registering OpenCode Blazor services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all OpenCode Blazor services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers:
    /// - Theme services (OpenCodeThemeService)
    /// - Session management services (OpenCodeSessionManager)
    /// - File services (FileTreeBuilder)
    /// - Diff services (DiffService)
    /// - PTY/Terminal services (PtyManager)
    ///
    /// Usage in Program.cs:
    /// <code>
    /// builder.Services.AddOpenCodeBlazor();
    /// builder.Services.AddMudServices(); // Also needed for MudBlazor
    /// </code>
    /// </remarks>
    public static IServiceCollection AddOpenCodeBlazor(this IServiceCollection services)
    {
        // Add theming
        services.AddOpenCodeTheming();

        // Add session management
        services.AddScoped<OpenCodeSessionManager>();

        // Add file services
        services.AddScoped<FileTreeBuilder>();

        // Add diff services
        services.AddScoped<DiffService>();

        // Add PTY/terminal services
        services.AddScoped<PtyManager>();

        return services;
    }

    /// <summary>
    /// Adds OpenCode Blazor services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenCodeBlazor(
        this IServiceCollection services,
        Action<OpenCodeBlazorOptions> configure)
    {
        var options = new OpenCodeBlazorOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddOpenCodeBlazor();

        return services;
    }
}

/// <summary>
/// Configuration options for OpenCode Blazor.
/// </summary>
public class OpenCodeBlazorOptions
{
    /// <summary>
    /// Whether to use dark mode by default.
    /// </summary>
    public bool DefaultDarkMode { get; set; } = true;

    /// <summary>
    /// Whether to enable terminal/PTY features.
    /// </summary>
    public bool EnableTerminal { get; set; } = true;

    /// <summary>
    /// Maximum number of concurrent PTY sessions.
    /// </summary>
    public int MaxPtySessions { get; set; } = 4;

    /// <summary>
    /// Whether to enable file attachment features.
    /// </summary>
    public bool EnableFileAttachments { get; set; } = true;

    /// <summary>
    /// Maximum file attachment size in bytes.
    /// </summary>
    public long MaxAttachmentSize { get; set; } = 10 * 1024 * 1024; // 10MB
}
