using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;
using AgUi.IDE.Desktop;
using Shared.Components;

namespace AgUi.IDE.Desktop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .AddMauiBlazorWebView();

            // TODO: Configure OpenCode Serve API client
            // builder.Services.AddScoped<IOpenCodeClient>(sp => ...);

            // TODO: Add project management services
            // builder.Services.AddScoped<IProjectManager>();

            // TODO: Add OpenCodeProcessManager service
            builder.Services.AddScoped<OpenCodeProcessManager>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            return builder.Build();
        }
    }
}
