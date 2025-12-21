using AgUi.Chat.BlazorWasm.Server;
using Microsoft.AspNetCore.ResponseCompression;

// TODO: Configure OpenCode Serve API integration
// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddOpenCodeServe(options => ...);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

// TODO: Add OpenCode Serve integration
// builder.Services.AddOpenCodeServe(options => ...);

var app = builder.Build();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AgUi.Chat.BlazorWasm.Client.Program).Assembly);

// TODO: Add API endpoints for OpenCode integration
// app.MapPost("/api/chat", ...);

app.MapFallbackToFile("index.html");

app.Run();
