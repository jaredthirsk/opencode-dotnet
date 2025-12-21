using Microsoft.Extensions.DependencyInjection;

namespace LionFire.OpenCode.Blazor.Theming;

/// <summary>
/// Extension methods for registering OpenCode theme services.
/// </summary>
public static class OpenCodeThemeExtensions
{
    /// <summary>
    /// Adds OpenCode theming services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This registers:
    /// - <see cref="OpenCodeThemeService"/> as a scoped service for theme state management
    ///
    /// Usage in Program.cs:
    /// <code>
    /// builder.Services.AddOpenCodeTheming();
    /// </code>
    ///
    /// Usage in a Blazor component:
    /// <code>
    /// @inject OpenCodeThemeService ThemeService
    ///
    /// protected override async Task OnAfterRenderAsync(bool firstRender)
    /// {
    ///     if (firstRender)
    ///     {
    ///         await ThemeService.InitializeAsync();
    ///         StateHasChanged();
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddOpenCodeTheming(this IServiceCollection services)
    {
        services.AddScoped<OpenCodeThemeService>();
        return services;
    }
}
