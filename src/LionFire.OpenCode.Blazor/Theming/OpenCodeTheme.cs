using MudBlazor;

namespace LionFire.OpenCode.Blazor.Theming;

/// <summary>
/// MudBlazor theme configuration that matches OpenCode's visual design.
/// </summary>
/// <remarks>
/// This theme is designed to work alongside the CSS custom properties defined in:
/// - opencode-colors.css (color palette)
/// - opencode-theme.css (semantic variables)
/// - opencode-base.css (global resets)
/// - opencode-utilities.css (utility classes)
///
/// For best results, ensure these CSS files are loaded before MudBlazor styles.
/// </remarks>
public static class OpenCodeTheme
{
    /// <summary>
    /// Gets the OpenCode MudBlazor theme (dark mode).
    /// </summary>
    public static MudTheme Theme => new()
    {
        PaletteLight = LightPalette,
        PaletteDark = DarkPalette,
        Typography = Typography,
        LayoutProperties = LayoutProperties,
        Shadows = Shadows,
        ZIndex = ZIndex,
    };

    /// <summary>
    /// Light mode palette matching OpenCode's light theme.
    /// </summary>
    private static PaletteLight LightPalette => new()
    {
        // Primary colors (using Cobalt)
        Primary = "#0167ff",           // cobalt-light-9
        PrimaryContrastText = "#ffffff",
        PrimaryDarken = "#004aff",     // cobalt-light-10
        PrimaryLighten = "#6ba3ff",    // cobalt-light-7

        // Secondary colors (using Smoke/Gray)
        Secondary = "#1c1917",         // smoke-light-12
        SecondaryContrastText = "#ffffff",
        SecondaryDarken = "#0f0d0c",
        SecondaryLighten = "#44403c",  // smoke-light-11

        // Tertiary/Accent (using Yuzu/Yellow for brand)
        Tertiary = "#ffd42e",          // yuzu-light-9
        TertiaryContrastText = "#1c1917",

        // Semantic colors
        Success = "#2b8e56",            // apple-light-9
        SuccessContrastText = "#ffffff",
        SuccessDarken = "#1f7341",      // apple-light-10
        SuccessLighten = "#94d4a1",     // apple-light-5

        Warning = "#f59e0b",            // solaris-light-9
        WarningContrastText = "#1c1917",
        WarningDarken = "#d97706",      // solaris-light-10
        WarningLighten = "#fcd34d",     // solaris-light-5

        Error = "#e53935",              // ember-light-9
        ErrorContrastText = "#ffffff",
        ErrorDarken = "#d32f2f",        // ember-light-10
        ErrorLighten = "#f28b82",       // ember-light-6

        Info = "#8b5cf6",               // lilac-light-9
        InfoContrastText = "#ffffff",
        InfoDarken = "#7c3aed",         // lilac-light-10
        InfoLighten = "#c4b5fd",        // lilac-light-5

        // Background colors
        Background = "#f8f7f7",         // --background-base (light)
        BackgroundGray = "#f5f5f4",     // smoke-light-2
        Surface = "#ffffff",            // --surface-strong

        // Text colors
        TextPrimary = "#1c1917",        // smoke-light-12 (--text-strong)
        TextSecondary = "#57534e",      // smoke-light-9 (--text-weak)
        TextDisabled = "#a8a29e",       // smoke-light-8 (--text-weaker)

        // Action/Interactive colors
        ActionDefault = "#57534e",      // smoke-light-9
        ActionDisabled = "#a8a29e80",   // smoke-light-8 with alpha
        ActionDisabledBackground = "#f5f5f4",

        // Drawer/Navigation
        DrawerBackground = "#f8f7f7",
        DrawerText = "#1c1917",
        DrawerIcon = "#57534e",

        // AppBar
        AppbarBackground = "#f8f7f7",
        AppbarText = "#1c1917",

        // Divider/Lines
        Divider = "#00000012",          // smoke-light-alpha-5
        DividerLight = "#00000008",     // smoke-light-alpha-3

        // Table
        TableLines = "#00000012",
        TableStriped = "#0000000a",
        TableHover = "#0000000f",

        // Overlay
        OverlayDark = "#000000a0",
        OverlayLight = "#ffffff80",
    };

    /// <summary>
    /// Dark mode palette matching OpenCode's dark theme.
    /// </summary>
    private static PaletteDark DarkPalette => new()
    {
        // Primary colors (using Cobalt dark)
        Primary = "#6ba3ff",            // cobalt-dark-9
        PrimaryContrastText = "#131010",
        PrimaryDarken = "#004aff",
        PrimaryLighten = "#92bdff",     // cobalt-dark-7

        // Secondary colors (using Smoke/Gray dark)
        Secondary = "#faf5f0",          // smoke-dark-12
        SecondaryContrastText = "#131010",
        SecondaryDarken = "#e6e1dc",
        SecondaryLighten = "#c8c3be",   // smoke-dark-10

        // Tertiary/Accent (using Yuzu/Yellow for brand)
        Tertiary = "#ffd42e",           // yuzu-dark-9
        TertiaryContrastText = "#131010",

        // Semantic colors (dark variants)
        Success = "#4ade80",            // apple-dark-9
        SuccessContrastText = "#131010",
        SuccessDarken = "#22c55e",      // apple-dark-10
        SuccessLighten = "#86efac",     // apple-dark-7

        Warning = "#fbbf24",            // solaris-dark-9
        WarningContrastText = "#131010",
        WarningDarken = "#f59e0b",      // solaris-dark-10
        WarningLighten = "#fcd34d",     // solaris-dark-7

        Error = "#f87171",              // ember-dark-9
        ErrorContrastText = "#131010",
        ErrorDarken = "#ef4444",        // ember-dark-10
        ErrorLighten = "#fca5a5",       // ember-dark-7

        Info = "#a78bfa",               // lilac-dark-9
        InfoContrastText = "#131010",
        InfoDarken = "#8b5cf6",         // lilac-dark-10
        InfoLighten = "#c4b5fd",        // lilac-dark-7

        // Background colors
        Background = "#131010",         // smoke-dark-1 (--background-base)
        BackgroundGray = "#1b1818",     // smoke-dark-2
        Surface = "#1b1818",            // smoke-dark-2

        // Text colors
        TextPrimary = "#faf5f0",        // smoke-dark-12 (--text-strong)
        TextSecondary = "#c8c3be",      // smoke-dark-9 (--text-weak)
        TextDisabled = "#a8a29e",       // smoke-dark-8 (--text-weaker)

        // Action/Interactive colors
        ActionDefault = "#c8c3be",      // smoke-dark-9
        ActionDisabled = "#a8a29e80",
        ActionDisabledBackground = "#1b1818",

        // Drawer/Navigation
        DrawerBackground = "#131010",
        DrawerText = "#faf5f0",
        DrawerIcon = "#c8c3be",

        // AppBar
        AppbarBackground = "#131010",
        AppbarText = "#faf5f0",

        // Divider/Lines
        Divider = "#ffffff1a",          // smoke-dark-alpha-6
        DividerLight = "#ffffff0d",     // smoke-dark-alpha-3

        // Table
        TableLines = "#ffffff1a",
        TableStriped = "#ffffff0d",
        TableHover = "#ffffff14",

        // Overlay
        OverlayDark = "#000000c0",
        OverlayLight = "#00000080",
    };

    /// <summary>
    /// Typography settings matching OpenCode's font system.
    /// </summary>
    private static Typography Typography => new()
    {
        Default = new Default
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "14px",
            FontWeight = 400,
            LineHeight = 1.5,
            LetterSpacing = "normal",
        },
        H1 = new H1
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "2.5rem",
            FontWeight = 600,
            LineHeight = 1.2,
            LetterSpacing = "-0.32px",
        },
        H2 = new H2
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "2rem",
            FontWeight = 600,
            LineHeight = 1.25,
            LetterSpacing = "-0.32px",
        },
        H3 = new H3
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "1.5rem",
            FontWeight = 600,
            LineHeight = 1.3,
            LetterSpacing = "-0.16px",
        },
        H4 = new H4
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "1.25rem",
            FontWeight = 500,
            LineHeight = 1.35,
            LetterSpacing = "-0.16px",
        },
        H5 = new H5
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "1.125rem",
            FontWeight = 500,
            LineHeight = 1.4,
            LetterSpacing = "normal",
        },
        H6 = new H6
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "1rem",
            FontWeight = 500,
            LineHeight = 1.45,
            LetterSpacing = "normal",
        },
        Body1 = new Body1
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "14px",
            FontWeight = 400,
            LineHeight = 1.5,
            LetterSpacing = "normal",
        },
        Body2 = new Body2
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "12px",
            FontWeight = 400,
            LineHeight = 1.5,
            LetterSpacing = "normal",
        },
        Subtitle1 = new Subtitle1
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "16px",
            FontWeight = 500,
            LineHeight = 1.5,
            LetterSpacing = "normal",
        },
        Subtitle2 = new Subtitle2
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "14px",
            FontWeight = 500,
            LineHeight = 1.5,
            LetterSpacing = "normal",
        },
        Button = new Button
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "14px",
            FontWeight = 500,
            LineHeight = 1.5,
            LetterSpacing = "normal",
            TextTransform = "none", // OpenCode doesn't use uppercase buttons
        },
        Caption = new Caption
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "12px",
            FontWeight = 400,
            LineHeight = 1.4,
            LetterSpacing = "normal",
        },
        Overline = new Overline
        {
            FontFamily = new[] { "Geist", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif" },
            FontSize = "10px",
            FontWeight = 500,
            LineHeight = 1.5,
            LetterSpacing = "0.5px",
            TextTransform = "uppercase",
        },
    };

    /// <summary>
    /// Layout properties for MudBlazor components.
    /// </summary>
    private static LayoutProperties LayoutProperties => new()
    {
        DefaultBorderRadius = "6px",    // --radius-md (0.375rem)
        DrawerWidthLeft = "256px",
        DrawerWidthRight = "256px",
        DrawerMiniWidthLeft = "56px",
        DrawerMiniWidthRight = "56px",
        AppbarHeight = "48px",
    };

    /// <summary>
    /// Shadow definitions matching OpenCode's design.
    /// </summary>
    private static Shadow Shadows => new()
    {
        // OpenCode shadows are subtle, using warm gray tones
        Elevation = new string[]
        {
            "none",
            "0 1px 2px -1px rgba(19, 16, 16, 0.04), 0 1px 2px 0 rgba(19, 16, 16, 0.06), 0 1px 3px 0 rgba(19, 16, 16, 0.08)",
            "0 2px 4px -1px rgba(19, 16, 16, 0.06), 0 2px 3px 0 rgba(19, 16, 16, 0.08), 0 1px 4px 0 rgba(19, 16, 16, 0.1)",
            "0 3px 6px -2px rgba(19, 16, 16, 0.08), 0 3px 4px -1px rgba(19, 16, 16, 0.1), 0 1px 5px 0 rgba(19, 16, 16, 0.12)",
            "0 6px 8px -4px rgba(19, 16, 16, 0.12), 0 4px 3px -2px rgba(19, 16, 16, 0.12), 0 1px 2px -1px rgba(19, 16, 16, 0.12)",
            "0 8px 10px -5px rgba(19, 16, 16, 0.14), 0 5px 4px -3px rgba(19, 16, 16, 0.12), 0 2px 3px -1px rgba(19, 16, 16, 0.1)",
            "0 10px 12px -6px rgba(19, 16, 16, 0.16), 0 6px 5px -3px rgba(19, 16, 16, 0.12), 0 2px 4px -1px rgba(19, 16, 16, 0.1)",
            "0 12px 14px -7px rgba(19, 16, 16, 0.18), 0 7px 6px -4px rgba(19, 16, 16, 0.12), 0 3px 5px -2px rgba(19, 16, 16, 0.1)",
            "0 14px 16px -8px rgba(19, 16, 16, 0.2), 0 8px 7px -4px rgba(19, 16, 16, 0.12), 0 3px 6px -2px rgba(19, 16, 16, 0.1)",
            "0 16px 18px -9px rgba(19, 16, 16, 0.22), 0 9px 8px -5px rgba(19, 16, 16, 0.12), 0 4px 7px -3px rgba(19, 16, 16, 0.1)",
            "0 18px 20px -10px rgba(19, 16, 16, 0.24), 0 10px 9px -5px rgba(19, 16, 16, 0.12), 0 4px 8px -3px rgba(19, 16, 16, 0.1)",
            "0 20px 22px -11px rgba(19, 16, 16, 0.26), 0 11px 10px -6px rgba(19, 16, 16, 0.12), 0 5px 9px -4px rgba(19, 16, 16, 0.1)",
            "0 22px 24px -12px rgba(19, 16, 16, 0.28), 0 12px 11px -6px rgba(19, 16, 16, 0.12), 0 5px 10px -4px rgba(19, 16, 16, 0.1)",
            "0 24px 26px -13px rgba(19, 16, 16, 0.3), 0 13px 12px -7px rgba(19, 16, 16, 0.12), 0 6px 11px -5px rgba(19, 16, 16, 0.1)",
            "0 26px 28px -14px rgba(19, 16, 16, 0.32), 0 14px 13px -7px rgba(19, 16, 16, 0.12), 0 6px 12px -5px rgba(19, 16, 16, 0.1)",
            "0 28px 30px -15px rgba(19, 16, 16, 0.34), 0 15px 14px -8px rgba(19, 16, 16, 0.12), 0 7px 13px -6px rgba(19, 16, 16, 0.1)",
            "0 30px 32px -16px rgba(19, 16, 16, 0.36), 0 16px 15px -8px rgba(19, 16, 16, 0.12), 0 7px 14px -6px rgba(19, 16, 16, 0.1)",
            "0 32px 34px -17px rgba(19, 16, 16, 0.38), 0 17px 16px -9px rgba(19, 16, 16, 0.12), 0 8px 15px -7px rgba(19, 16, 16, 0.1)",
            "0 34px 36px -18px rgba(19, 16, 16, 0.4), 0 18px 17px -9px rgba(19, 16, 16, 0.12), 0 8px 16px -7px rgba(19, 16, 16, 0.1)",
            "0 36px 38px -19px rgba(19, 16, 16, 0.42), 0 19px 18px -10px rgba(19, 16, 16, 0.12), 0 9px 17px -8px rgba(19, 16, 16, 0.1)",
            "0 38px 40px -20px rgba(19, 16, 16, 0.44), 0 20px 19px -10px rgba(19, 16, 16, 0.12), 0 9px 18px -8px rgba(19, 16, 16, 0.1)",
            "0 40px 42px -21px rgba(19, 16, 16, 0.46), 0 21px 20px -11px rgba(19, 16, 16, 0.12), 0 10px 19px -9px rgba(19, 16, 16, 0.1)",
            "0 42px 44px -22px rgba(19, 16, 16, 0.48), 0 22px 21px -11px rgba(19, 16, 16, 0.12), 0 10px 20px -9px rgba(19, 16, 16, 0.1)",
            "0 44px 46px -23px rgba(19, 16, 16, 0.5), 0 23px 22px -12px rgba(19, 16, 16, 0.12), 0 11px 21px -10px rgba(19, 16, 16, 0.1)",
            "0 46px 48px -24px rgba(19, 16, 16, 0.52), 0 24px 23px -12px rgba(19, 16, 16, 0.12), 0 11px 22px -10px rgba(19, 16, 16, 0.1)",
            "0 48px 50px -25px rgba(19, 16, 16, 0.54), 0 25px 24px -13px rgba(19, 16, 16, 0.12), 0 12px 23px -11px rgba(19, 16, 16, 0.1)",
        },
    };

    /// <summary>
    /// Z-index configuration.
    /// </summary>
    private static ZIndex ZIndex => new()
    {
        Drawer = 1100,
        Popover = 1200,
        AppBar = 1300,
        Dialog = 1400,
        Snackbar = 1500,
        Tooltip = 1600,
    };
}
