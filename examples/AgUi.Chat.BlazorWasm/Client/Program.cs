using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AgUi.Chat.BlazorWasm.Client;

// TODO: Configure HttpClient for API calls
// TODO: Setup OpenCode client service
// var builder = WebAssemblyHostBuilder.CreateDefault(args);
// builder.Services.AddScoped<IOpenCodeClient>(sp => ...);

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// TODO: Add scoped HttpClient for OpenCode API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
