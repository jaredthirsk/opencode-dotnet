using Microsoft.Extensions.AI;

namespace LionFire.OpenCode.Serve.AgentFramework;

/// <summary>
/// Extension methods for integrating OpenCode with Microsoft.Extensions.AI.
/// </summary>
public static class OpenCodeClientExtensions
{
    /// <summary>
    /// Creates an <see cref="IChatClient"/> from the OpenCode client.
    /// </summary>
    /// <param name="client">The OpenCode client.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A chat client implementation.</returns>
    /// <example>
    /// <code>
    /// IChatClient chatClient = openCodeClient.AsChatClient();
    /// var response = await chatClient.GetResponseAsync([new ChatMessage(ChatRole.User, "Hello!")]);
    /// </code>
    /// </example>
    public static IChatClient AsChatClient(
        this IOpenCodeClient client,
        OpenCodeChatClientOptions? options = null)
    {
        return new OpenCodeChatClient(client, options);
    }

    /// <summary>
    /// Creates an <see cref="IChatClient"/> with a specific session.
    /// </summary>
    /// <param name="client">The OpenCode client.</param>
    /// <param name="sessionId">The session ID to use.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A chat client implementation bound to the session.</returns>
    public static IChatClient AsChatClient(
        this IOpenCodeClient client,
        string sessionId,
        OpenCodeChatClientOptions? options = null)
    {
        var chatClient = new OpenCodeChatClient(client, options)
        {
            SessionId = sessionId
        };
        return chatClient;
    }

    /// <summary>
    /// Creates an <see cref="IChatClient"/> from a session scope.
    /// The chat client will use the scope's session and will not clean it up.
    /// </summary>
    /// <param name="scope">The session scope.</param>
    /// <param name="client">The OpenCode client.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A chat client implementation bound to the session scope.</returns>
    public static IChatClient AsChatClient(
        this ISessionScope scope,
        IOpenCodeClient client,
        OpenCodeChatClientOptions? options = null)
    {
        return new OpenCodeChatClient(client, options)
        {
            SessionId = scope.SessionId
        };
    }
}
