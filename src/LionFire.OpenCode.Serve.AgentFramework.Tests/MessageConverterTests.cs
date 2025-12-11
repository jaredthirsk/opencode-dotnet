using FluentAssertions;
using Microsoft.Extensions.AI;
using Xunit;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.AgentFramework;

namespace LionFire.OpenCode.Serve.AgentFramework.Tests;

public class MessageConverterTests
{
    private static readonly DateTimeOffset TestDate = DateTimeOffset.Parse("2024-01-15T10:00:00Z");

    [Fact]
    public void ToChatMessage_WithNullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToChatMessage(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Theory]
    [InlineData(MessageRole.User)]
    [InlineData(MessageRole.Assistant)]
    [InlineData(MessageRole.System)]
    [InlineData(MessageRole.Tool)]
    public void ToChatMessage_ConvertsRole(MessageRole role)
    {
        // Arrange
        var message = new Message("id", "session", role, new[] { new TextPart("test") }, TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(message);

        // Assert
        chatMessage.Role.Should().Be(MessageConverter.ToChatRole(role));
    }

    [Fact]
    public void ToChatMessage_ConvertsTextPart()
    {
        // Arrange
        var message = new Message(
            "id", "session",
            MessageRole.Assistant,
            new[] { new TextPart("Hello, world!") },
            TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(message);

        // Assert
        chatMessage.Contents.Should().HaveCount(1);
        chatMessage.Contents[0].Should().BeOfType<TextContent>();
        ((TextContent)chatMessage.Contents[0]).Text.Should().Be("Hello, world!");
    }

    [Fact]
    public void ToChatMessage_ConvertsToolUsePart()
    {
        // Arrange
        var input = new Dictionary<string, object?> { ["arg1"] = "value1" };
        var message = new Message(
            "id", "session",
            MessageRole.Assistant,
            new[] { new ToolUsePart("tool-123", "searchFiles", input) },
            TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(message);

        // Assert
        chatMessage.Contents.Should().HaveCount(1);
        chatMessage.Contents[0].Should().BeOfType<FunctionCallContent>();
        var functionCall = (FunctionCallContent)chatMessage.Contents[0];
        functionCall.CallId.Should().Be("tool-123");
        functionCall.Name.Should().Be("searchFiles");
    }

    [Fact]
    public void ToChatMessage_ConvertsToolResultPart()
    {
        // Arrange
        var message = new Message(
            "id", "session",
            MessageRole.Tool,
            new[] { new ToolResultPart("tool-456", "File found: test.cs") },
            TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(message);

        // Assert
        chatMessage.Contents.Should().HaveCount(1);
        chatMessage.Contents[0].Should().BeOfType<FunctionResultContent>();
        var functionResult = (FunctionResultContent)chatMessage.Contents[0];
        functionResult.CallId.Should().Be("tool-456");
        functionResult.Result.Should().Be("File found: test.cs");
    }

    [Fact]
    public void ToChatMessage_ConvertsMultipleParts()
    {
        // Arrange
        var message = new Message(
            "id", "session",
            MessageRole.Assistant,
            new MessagePart[]
            {
                new TextPart("Let me search for that."),
                new ToolUsePart("call-1", "search", null)
            },
            TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(message);

        // Assert
        chatMessage.Contents.Should().HaveCount(2);
        chatMessage.Contents[0].Should().BeOfType<TextContent>();
        chatMessage.Contents[1].Should().BeOfType<FunctionCallContent>();
    }

    [Fact]
    public void ToMessageParts_WithNullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToMessageParts(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void ToMessageParts_ConvertsTextContent()
    {
        // Arrange
        var chatMessage = new ChatMessage(ChatRole.User, "Hello!");

        // Act
        var parts = MessageConverter.ToMessageParts(chatMessage);

        // Assert
        parts.Should().HaveCount(1);
        parts[0].Should().BeOfType<TextPart>();
        ((TextPart)parts[0]).Text.Should().Be("Hello!");
    }

    [Fact]
    public void ToMessageParts_ConvertsFunctionCallContent()
    {
        // Arrange
        var args = new Dictionary<string, object?> { ["query"] = "test" };
        var chatMessage = new ChatMessage
        {
            Role = ChatRole.Assistant,
            Contents = { new FunctionCallContent("call-id", "functionName", args) }
        };

        // Act
        var parts = MessageConverter.ToMessageParts(chatMessage);

        // Assert
        parts.Should().HaveCount(1);
        parts[0].Should().BeOfType<ToolUsePart>();
        var toolUse = (ToolUsePart)parts[0];
        toolUse.ToolId.Should().Be("call-id");
        toolUse.ToolName.Should().Be("functionName");
    }

    [Fact]
    public void ToMessageParts_ConvertsFunctionResultContent()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            Role = ChatRole.Tool,
            Contents = { new FunctionResultContent("call-id", "result data") }
        };

        // Act
        var parts = MessageConverter.ToMessageParts(chatMessage);

        // Assert
        parts.Should().HaveCount(1);
        parts[0].Should().BeOfType<ToolResultPart>();
        var toolResult = (ToolResultPart)parts[0];
        toolResult.ToolId.Should().Be("call-id");
        toolResult.Output.Should().Be("result data");
    }

    [Fact]
    public void ToChatResponse_WithNullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToChatResponse(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void ToChatResponse_SetsResponseIdAndCreatedAt()
    {
        // Arrange
        var createdAt = DateTimeOffset.Parse("2024-06-15T14:30:00Z");
        var message = new Message(
            "response-123", "session",
            MessageRole.Assistant,
            new[] { new TextPart("Response") },
            createdAt);

        // Act
        var response = MessageConverter.ToChatResponse(message);

        // Assert
        response.ResponseId.Should().Be("response-123");
        response.CreatedAt.Should().Be(createdAt);
        response.Messages.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(MessageRole.User)]
    [InlineData(MessageRole.Assistant)]
    [InlineData(MessageRole.System)]
    [InlineData(MessageRole.Tool)]
    public void ToChatRole_ConvertsCorrectly(MessageRole messageRole)
    {
        // Act
        var chatRole = MessageConverter.ToChatRole(messageRole);

        // Assert
        var expectedRole = messageRole switch
        {
            MessageRole.User => ChatRole.User,
            MessageRole.Assistant => ChatRole.Assistant,
            MessageRole.System => ChatRole.System,
            MessageRole.Tool => ChatRole.Tool,
            _ => ChatRole.Assistant
        };
        chatRole.Should().Be(expectedRole);
    }

    [Fact]
    public void ToMessageRole_ConvertsUserRole()
    {
        MessageConverter.ToMessageRole(ChatRole.User).Should().Be(MessageRole.User);
    }

    [Fact]
    public void ToMessageRole_ConvertsAssistantRole()
    {
        MessageConverter.ToMessageRole(ChatRole.Assistant).Should().Be(MessageRole.Assistant);
    }

    [Fact]
    public void ToMessageRole_ConvertsSystemRole()
    {
        MessageConverter.ToMessageRole(ChatRole.System).Should().Be(MessageRole.System);
    }

    [Fact]
    public void ToMessageRole_ConvertsToolRole()
    {
        MessageConverter.ToMessageRole(ChatRole.Tool).Should().Be(MessageRole.Tool);
    }

    [Fact]
    public void ToChatMessages_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToChatMessages(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("messages");
    }

    [Fact]
    public void ToChatMessages_ConvertsMultipleMessages()
    {
        // Arrange
        var messages = new[]
        {
            new Message("1", "session", MessageRole.User, new[] { new TextPart("Hello") }, TestDate),
            new Message("2", "session", MessageRole.Assistant, new[] { new TextPart("Hi there!") }, TestDate)
        };

        // Act
        var chatMessages = MessageConverter.ToChatMessages(messages);

        // Assert
        chatMessages.Should().HaveCount(2);
        chatMessages[0].Role.Should().Be(ChatRole.User);
        chatMessages[1].Role.Should().Be(ChatRole.Assistant);
    }

    [Fact]
    public void ToChatResponseUpdate_WithNullUpdate_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToChatResponseUpdate(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("update");
    }

    [Fact]
    public void ToChatResponseUpdate_WithDelta_CreatesUpdate()
    {
        // Arrange
        var update = new MessageUpdate { Delta = "Hello", MessageId = "msg-1" };

        // Act
        var chatUpdate = MessageConverter.ToChatResponseUpdate(update);

        // Assert
        chatUpdate.Should().NotBeNull();
        chatUpdate!.Text.Should().Be("Hello");
        chatUpdate.MessageId.Should().Be("msg-1");
        chatUpdate.Role.Should().Be(ChatRole.Assistant);
    }

    [Fact]
    public void ToChatResponseUpdate_WithDone_SetsFinishReason()
    {
        // Arrange
        var update = new MessageUpdate { Delta = "Final", MessageId = "msg-1", Done = true };

        // Act
        var chatUpdate = MessageConverter.ToChatResponseUpdate(update);

        // Assert
        chatUpdate.Should().NotBeNull();
        chatUpdate!.FinishReason.Should().Be(ChatFinishReason.Stop);
    }

    [Fact]
    public void ToChatResponseUpdate_WithDoneOnly_CreatesStopUpdate()
    {
        // Arrange
        var update = new MessageUpdate { Done = true, MessageId = "msg-1" };

        // Act
        var chatUpdate = MessageConverter.ToChatResponseUpdate(update);

        // Assert
        chatUpdate.Should().NotBeNull();
        chatUpdate!.FinishReason.Should().Be(ChatFinishReason.Stop);
        chatUpdate.MessageId.Should().Be("msg-1");
    }

    [Fact]
    public void ToChatResponseUpdate_WithNoDeltaOrDone_ReturnsNull()
    {
        // Arrange
        var update = new MessageUpdate { MessageId = "msg-1" };

        // Act
        var chatUpdate = MessageConverter.ToChatResponseUpdate(update);

        // Assert
        chatUpdate.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_TextMessage_PreservesContent()
    {
        // Arrange
        var originalMessage = new Message(
            "id", "session",
            MessageRole.User,
            new[] { new TextPart("Round trip test") },
            TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(originalMessage);
        var parts = MessageConverter.ToMessageParts(chatMessage);

        // Assert
        parts.Should().HaveCount(1);
        parts[0].Should().BeOfType<TextPart>();
        ((TextPart)parts[0]).Text.Should().Be("Round trip test");
    }

    [Fact]
    public void RoundTrip_ToolCall_PreservesContent()
    {
        // Arrange
        var input = new Dictionary<string, object?> { ["param"] = "value" };
        var originalMessage = new Message(
            "id", "session",
            MessageRole.Assistant,
            new[] { new ToolUsePart("tool-id", "toolName", input) },
            TestDate);

        // Act
        var chatMessage = MessageConverter.ToChatMessage(originalMessage);
        var parts = MessageConverter.ToMessageParts(chatMessage);

        // Assert
        parts.Should().HaveCount(1);
        var toolUse = parts[0].Should().BeOfType<ToolUsePart>().Subject;
        toolUse.ToolId.Should().Be("tool-id");
        toolUse.ToolName.Should().Be("toolName");
    }
}
