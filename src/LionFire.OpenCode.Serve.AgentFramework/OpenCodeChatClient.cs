using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

namespace LionFire.OpenCode.Serve.AgentFramework;

/// <summary>
/// An implementation of <see cref="IChatClient"/> that wraps an OpenCode session.
/// Enables using OpenCode as a chat backend through the Microsoft.Extensions.AI abstractions.
/// </summary>
public class OpenCodeChatClient : IChatClient
{
    private readonly IOpenCodeClient _client;
    private readonly OpenCodeChatClientOptions _options;
    private string? _sessionId;

    /// <summary>
    /// Gets the metadata for this chat client.
    /// </summary>
    public ChatClientMetadata Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenCodeChatClient"/> class.
    /// </summary>
    /// <param name="client">The OpenCode client.</param>
    /// <param name="options">Optional configuration options.</param>
    public OpenCodeChatClient(IOpenCodeClient client, OpenCodeChatClientOptions? options = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? new OpenCodeChatClientOptions();

        Metadata = new ChatClientMetadata(
            "OpenCode",
            new Uri(_options.BaseUrl ?? OpenCodeClientOptions.DefaultBaseUrl),
            _options.ModelId);
    }

    /// <summary>
    /// Gets or sets the session ID for this chat client.
    /// If not set, a new session will be created on the first message.
    /// </summary>
    public string? SessionId
    {
        get => _sessionId;
        set => _sessionId = value;
    }

    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatMessages);

        // Ensure we have a session
        if (_sessionId is null)
        {
            var session = await _client.CreateSessionAsync(_options.Directory, cancellationToken);
            _sessionId = session.Id;
        }

        // Get the last user message
        var messagesList = chatMessages.ToList();
        var lastMessage = messagesList.LastOrDefault(m => m.Role == ChatRole.User);
        if (lastMessage is null)
        {
            throw new ArgumentException("No user message found in chat messages", nameof(chatMessages));
        }

        // Convert to OpenCode message parts
        var parts = ConvertToParts(lastMessage);

        // Send and get response
        var response = await _client.SendMessageAsync(_sessionId, parts, cancellationToken);

        // Convert back to ChatResponse
        return ConvertToResponse(response);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatMessages);

        // Ensure we have a session
        if (_sessionId is null)
        {
            var session = await _client.CreateSessionAsync(_options.Directory, cancellationToken);
            _sessionId = session.Id;
        }

        // Get the last user message
        var messagesList = chatMessages.ToList();
        var lastMessage = messagesList.LastOrDefault(m => m.Role == ChatRole.User);
        if (lastMessage is null)
        {
            throw new ArgumentException("No user message found in chat messages", nameof(chatMessages));
        }

        // Convert to OpenCode message parts
        var parts = ConvertToParts(lastMessage);

        // Stream response
        await foreach (var update in _client.SendMessageStreamingAsync(_sessionId, parts, cancellationToken))
        {
            if (update.Delta is not null)
            {
                yield return new ChatResponseUpdate(ChatRole.Assistant, update.Delta)
                {
                    MessageId = update.MessageId
                };
            }

            if (update.Done)
            {
                yield return new ChatResponseUpdate
                {
                    Role = ChatRole.Assistant,
                    FinishReason = ChatFinishReason.Stop,
                    MessageId = update.MessageId
                };
            }
        }
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey is not null)
        {
            return null;
        }

        if (serviceType == typeof(IOpenCodeClient))
        {
            return _client;
        }

        if (serviceType == typeof(OpenCodeChatClient))
        {
            return this;
        }

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Session cleanup is handled by the user via SessionId management
        // or using ISessionScope in the IOpenCodeClient
    }

    private IReadOnlyList<MessagePart> ConvertToParts(ChatMessage message)
    {
        var parts = new List<MessagePart>();

        foreach (var content in message.Contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    parts.Add(new TextPart(textContent.Text ?? string.Empty));
                    break;

                // Add more content type handlers as needed
                default:
                    if (content is { } anyContent)
                    {
                        parts.Add(new TextPart(anyContent.ToString() ?? string.Empty));
                    }
                    break;
            }
        }

        return parts;
    }

    private ChatResponse ConvertToResponse(Message message)
    {
        var chatMessage = new ChatMessage
        {
            Role = message.Role switch
            {
                MessageRole.User => ChatRole.User,
                MessageRole.Assistant => ChatRole.Assistant,
                MessageRole.System => ChatRole.System,
                MessageRole.Tool => ChatRole.Tool,
                _ => ChatRole.Assistant
            }
        };

        foreach (var part in message.Parts)
        {
            switch (part)
            {
                case TextPart textPart:
                    chatMessage.Contents.Add(new TextContent(textPart.Text));
                    break;

                case ToolUsePart toolUse:
                    chatMessage.Contents.Add(new FunctionCallContent(
                        toolUse.ToolId,
                        toolUse.ToolName,
                        toolUse.Input as IDictionary<string, object?>));
                    break;

                case ToolResultPart toolResult:
                    chatMessage.Contents.Add(new FunctionResultContent(
                        toolResult.ToolId,
                        toolResult.Output));
                    break;
            }
        }

        return new ChatResponse(chatMessage)
        {
            ResponseId = message.Id,
            CreatedAt = message.CreatedAt
        };
    }
}

/// <summary>
/// Options for configuring the <see cref="OpenCodeChatClient"/>.
/// </summary>
public class OpenCodeChatClientOptions
{
    /// <summary>
    /// Gets or sets the working directory for sessions.
    /// </summary>
    public string? Directory { get; set; }

    /// <summary>
    /// Gets or sets the model ID to report in metadata.
    /// </summary>
    public string? ModelId { get; set; } = "opencode";

    /// <summary>
    /// Gets or sets the base URL to report in metadata.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets whether to create a new session for each conversation.
    /// Defaults to false (reuse session).
    /// </summary>
    public bool CreateSessionPerConversation { get; set; }
}
