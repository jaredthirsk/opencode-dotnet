using Microsoft.Extensions.AI;
using LionFire.OpenCode.Serve.Models;

namespace LionFire.OpenCode.Serve.AgentFramework;

/// <summary>
/// Utility class for converting between Microsoft.Extensions.AI messages and OpenCode messages.
/// </summary>
public static class MessageConverter
{
    /// <summary>
    /// Converts an OpenCode <see cref="Message"/> to a <see cref="ChatMessage"/>.
    /// </summary>
    /// <param name="message">The OpenCode message to convert.</param>
    /// <returns>A ChatMessage with equivalent content.</returns>
    public static ChatMessage ToChatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var chatMessage = new ChatMessage
        {
            Role = ToChatRole(message.Role)
        };

        foreach (var part in message.Parts)
        {
            var content = ToAIContent(part);
            if (content is not null)
            {
                chatMessage.Contents.Add(content);
            }
        }

        return chatMessage;
    }

    /// <summary>
    /// Converts a <see cref="ChatMessage"/> to OpenCode message parts.
    /// </summary>
    /// <param name="message">The ChatMessage to convert.</param>
    /// <returns>A list of message parts representing the content.</returns>
    public static IReadOnlyList<MessagePart> ToMessageParts(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var parts = new List<MessagePart>();

        foreach (var content in message.Contents)
        {
            var part = ToMessagePart(content);
            if (part is not null)
            {
                parts.Add(part);
            }
        }

        return parts;
    }

    /// <summary>
    /// Converts an OpenCode <see cref="Message"/> to a <see cref="ChatResponse"/>.
    /// </summary>
    /// <param name="message">The OpenCode message to convert.</param>
    /// <returns>A ChatResponse containing the message.</returns>
    public static ChatResponse ToChatResponse(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var chatMessage = ToChatMessage(message);

        return new ChatResponse(chatMessage)
        {
            ResponseId = message.Id,
            CreatedAt = message.CreatedAt
        };
    }

    /// <summary>
    /// Converts an OpenCode <see cref="MessageRole"/> to a <see cref="ChatRole"/>.
    /// </summary>
    /// <param name="role">The OpenCode message role.</param>
    /// <returns>The equivalent ChatRole.</returns>
    public static ChatRole ToChatRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.User => ChatRole.User,
            MessageRole.Assistant => ChatRole.Assistant,
            MessageRole.System => ChatRole.System,
            MessageRole.Tool => ChatRole.Tool,
            _ => ChatRole.Assistant
        };
    }

    /// <summary>
    /// Converts a <see cref="ChatRole"/> to an OpenCode <see cref="MessageRole"/>.
    /// </summary>
    /// <param name="role">The ChatRole to convert.</param>
    /// <returns>The equivalent MessageRole.</returns>
    public static MessageRole ToMessageRole(ChatRole role)
    {
        if (role == ChatRole.User)
            return MessageRole.User;
        if (role == ChatRole.Assistant)
            return MessageRole.Assistant;
        if (role == ChatRole.System)
            return MessageRole.System;
        if (role == ChatRole.Tool)
            return MessageRole.Tool;

        return MessageRole.User;
    }

    /// <summary>
    /// Converts an OpenCode <see cref="MessagePart"/> to an <see cref="AIContent"/>.
    /// </summary>
    /// <param name="part">The message part to convert.</param>
    /// <returns>The equivalent AIContent, or null if the part type is not supported.</returns>
    public static AIContent? ToAIContent(MessagePart part)
    {
        return part switch
        {
            TextPart textPart => new TextContent(textPart.Text),
            ToolUsePart toolUse => new FunctionCallContent(
                toolUse.ToolId,
                toolUse.ToolName,
                toolUse.Input as IDictionary<string, object?>),
            ToolResultPart toolResult => new FunctionResultContent(
                toolResult.ToolId,
                toolResult.Output),
            _ => null
        };
    }

    /// <summary>
    /// Converts an <see cref="AIContent"/> to an OpenCode <see cref="MessagePart"/>.
    /// </summary>
    /// <param name="content">The AI content to convert.</param>
    /// <returns>The equivalent MessagePart, or null if the content type is not supported.</returns>
    public static MessagePart? ToMessagePart(AIContent content)
    {
        return content switch
        {
            TextContent textContent => new TextPart(textContent.Text ?? string.Empty),
            FunctionCallContent functionCall => new ToolUsePart(
                functionCall.CallId,
                functionCall.Name,
                functionCall.Arguments as object),
            FunctionResultContent functionResult => new ToolResultPart(
                functionResult.CallId,
                functionResult.Result?.ToString()),
            _ => new TextPart(content.ToString() ?? string.Empty)
        };
    }

    /// <summary>
    /// Converts a collection of OpenCode messages to ChatMessages.
    /// </summary>
    /// <param name="messages">The messages to convert.</param>
    /// <returns>A list of converted ChatMessages.</returns>
    public static IReadOnlyList<ChatMessage> ToChatMessages(IEnumerable<Message> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        return messages.Select(ToChatMessage).ToList();
    }

    /// <summary>
    /// Converts an OpenCode <see cref="MessageUpdate"/> to a <see cref="ChatResponseUpdate"/>.
    /// </summary>
    /// <param name="update">The message update to convert.</param>
    /// <returns>A ChatResponseUpdate, or null if there's no content.</returns>
    public static ChatResponseUpdate? ToChatResponseUpdate(MessageUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (update.Delta is not null)
        {
            return new ChatResponseUpdate(ChatRole.Assistant, update.Delta)
            {
                MessageId = update.MessageId,
                FinishReason = update.Done ? ChatFinishReason.Stop : null
            };
        }

        if (update.Done)
        {
            return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                MessageId = update.MessageId,
                FinishReason = ChatFinishReason.Stop
            };
        }

        return null;
    }
}
