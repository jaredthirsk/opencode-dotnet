using Microsoft.JSInterop;

namespace LionFire.OpenCode.Blazor.Theming;

/// <summary>
/// Service for managing OpenCode theme state (dark/light mode).
/// </summary>
public class OpenCodeThemeService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode = true; // OpenCode defaults to dark mode
    private IJSObjectReference? _module;

    /// <summary>
    /// Event raised when the theme mode changes.
    /// </summary>
    public event Action<bool>? OnThemeChanged;

    /// <summary>
    /// Gets whether dark mode is currently active.
    /// </summary>
    public bool IsDarkMode => _isDarkMode;

    /// <summary>
    /// Gets whether light mode is currently active.
    /// </summary>
    public bool IsLightMode => !_isDarkMode;

    public OpenCodeThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initialize the theme service by reading the stored preference.
    /// Call this in OnAfterRenderAsync(firstRender: true).
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Try to read stored preference from localStorage
            var stored = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "opencode-theme");

            if (!string.IsNullOrEmpty(stored))
            {
                _isDarkMode = stored == "dark";
            }
            else
            {
                // Check system preference
                _isDarkMode = await _jsRuntime.InvokeAsync<bool>(
                    "eval",
                    "window.matchMedia('(prefers-color-scheme: dark)').matches");
            }

            await ApplyThemeToDocumentAsync();
        }
        catch
        {
            // Fallback to dark mode if JS fails (SSR scenario)
            _isDarkMode = true;
        }
    }

    /// <summary>
    /// Toggle between dark and light mode.
    /// </summary>
    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await SaveAndApplyThemeAsync();
    }

    /// <summary>
    /// Set the theme mode explicitly.
    /// </summary>
    public async Task SetDarkModeAsync(bool isDark)
    {
        if (_isDarkMode == isDark) return;

        _isDarkMode = isDark;
        await SaveAndApplyThemeAsync();
    }

    private async Task SaveAndApplyThemeAsync()
    {
        try
        {
            // Save to localStorage
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                "opencode-theme",
                _isDarkMode ? "dark" : "light");

            await ApplyThemeToDocumentAsync();
        }
        catch
        {
            // Ignore JS errors
        }

        OnThemeChanged?.Invoke(_isDarkMode);
    }

    private async Task ApplyThemeToDocumentAsync()
    {
        try
        {
            // Add/remove class on document element for CSS variable switching
            if (_isDarkMode)
            {
                await _jsRuntime.InvokeVoidAsync(
                    "eval",
                    "document.documentElement.classList.remove('light'); document.documentElement.classList.add('dark'); document.documentElement.dataset.theme = 'dark';");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync(
                    "eval",
                    "document.documentElement.classList.remove('dark'); document.documentElement.classList.add('light'); document.documentElement.dataset.theme = 'light';");
            }
        }
        catch
        {
            // Ignore JS errors (SSR scenario)
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
