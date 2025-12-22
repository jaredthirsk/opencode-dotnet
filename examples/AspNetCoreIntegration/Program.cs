// ASP.NET Core Integration Example
// Demonstrates using the SDK with dependency injection in ASP.NET Core

using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.AgentFramework;
using LionFire.OpenCode.Serve.Exceptions;
using LionFire.OpenCode.Serve.Extensions;
using LionFire.OpenCode.Serve.Models;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Register the OpenCode client with DI
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = builder.Configuration.GetValue<string>("OpenCode:BaseUrl")
        ?? "http://localhost:9123";
    options.EnableTelemetry = true;
});

// Register the IChatClient implementation
builder.Services.AddOpenCodeChatClient(chatOptions =>
{
    chatOptions.ModelId = "opencode";
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<OpenCodeHealthCheck>("opencode");

var app = builder.Build();

// Health check endpoint
app.MapHealthChecks("/health");

// Simple chat endpoint using IOpenCodeClient directly
app.MapPost("/api/chat", async (
    ChatRequest request,
    IOpenCodeClient client,
    CancellationToken cancellationToken) =>
{
    var session = await client.CreateSessionAsync(cancellationToken: cancellationToken);

    try
    {
        var messageRequest = new SendMessageRequest
        {
            Parts = new List<PartInput> { PartInput.TextInput(request.Message) }
        };

        var response = await client.PromptAsync(session.Id, messageRequest,
            cancellationToken: cancellationToken);

        var text = response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text ?? "";

        return Results.Ok(new ChatResponse(text, response.Message?.Id ?? ""));
    }
    finally
    {
        await client.DeleteSessionAsync(session.Id, cancellationToken: cancellationToken);
    }
});

// Chat endpoint using IChatClient (Microsoft.Extensions.AI)
app.MapPost("/api/chat/ai", async (
    ChatRequest request,
    IChatClient chatClient,
    CancellationToken cancellationToken) =>
{
    var messages = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.User, request.Message)
    };

    var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);

    var text = response.Messages.FirstOrDefault()?.Text ?? "";

    return Results.Ok(new ChatResponse(text, response.ResponseId ?? ""));
});

// List sessions endpoint
app.MapGet("/api/sessions", async (IOpenCodeClient client) =>
{
    var sessions = await client.ListSessionsAsync();
    return Results.Ok(sessions.Select(s => new
    {
        s.Id,
        s.Title,
        Created = DateTimeOffset.FromUnixTimeMilliseconds(s.Time.Created)
    }));
});

Console.WriteLine("ASP.NET Core Integration Example");
Console.WriteLine("================================");
Console.WriteLine();
Console.WriteLine("Endpoints:");
Console.WriteLine("  POST /api/chat       - Chat using IOpenCodeClient");
Console.WriteLine("  POST /api/chat/ai    - Chat using IChatClient");
Console.WriteLine("  GET  /api/sessions   - List sessions");
Console.WriteLine("  GET  /health         - Health check");
Console.WriteLine();

app.Run();

// Request/Response DTOs
record ChatRequest(string Message);
record ChatResponse(string Response, string MessageId);

// Health check implementation
public class OpenCodeHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly IOpenCodeClient _client;

    public OpenCodeHealthCheck(IOpenCodeClient client)
    {
        _client = client;
    }

    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.ListProjectsAsync(cancellationToken: cancellationToken);
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("OpenCode server is responding");
        }
        catch (OpenCodeConnectionException)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Cannot connect to OpenCode server");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
