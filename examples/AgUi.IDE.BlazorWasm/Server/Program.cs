using AgUi.IDE.BlazorWasm.Server;
using Microsoft.AspNetCore.ResponseCompression;

// TODO: Configure OpenCode Serve API integration
// TODO: Implement project management services
// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddOpenCodeServe(options => ...);
// builder.Services.AddScoped<IProjectManager>();

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

// TODO: Add project and file management services
// builder.Services.AddScoped<IProjectManager>();
// builder.Services.AddScoped<IFileService>();

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
    .AddAdditionalAssemblies(typeof(AgUi.IDE.BlazorWasm.Client.Program).Assembly);

// TODO: Add API endpoints for project and file operations
// app.MapGet("/api/projects", ...);
// app.MapGet("/api/projects/{projectId}/files", ...);
// app.MapGet("/api/files/{fileId}/content", ...);

app.MapFallbackToFile("index.html");

app.Run();
