using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AgUi.IDE.BlazorWasm.Client;
using Shared.Components;

// TODO: Configure HttpClient for API calls
// TODO: Setup OpenCode client service
// TODO: Setup project management service
// var builder = WebAssemblyHostBuilder.CreateDefault(args);
// builder.Services.AddScoped<IOpenCodeClient>(sp => ...);
// builder.Services.AddScoped<IProjectManager>(sp => ...);

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// TODO: Add scoped HttpClient for OpenCode API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// TODO: Add project management services
// builder.Services.AddScoped<IProjectManager>();
// builder.Services.AddScoped<IFileService>();

await builder.Build().RunAsync();
