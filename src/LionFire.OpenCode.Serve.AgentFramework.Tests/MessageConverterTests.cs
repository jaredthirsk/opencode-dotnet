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
            .WithParameterName("messageWithParts");
    }

    [Theory]
    [InlineData("user")]
    [InlineData("assistant")]
    [InlineData("system")]
    [InlineData("tool")]
    public void ToChatMessage_ConvertsRole(string role)
    {
        // Arrange
        var message = CreateUserMessage("id", "session", role);
        var messageWithParts = new MessageWithParts
        {
            Message = message,
            Parts = new List<Part> { CreateTextPart("test") }
        };

        // Act
        var chatMessage = MessageConverter.ToChatMessage(messageWithParts);

        // Assert
        chatMessage.Role.Should().Be(MessageConverter.ToChatRole(role));
    }

    [Fact]
    public void ToChatMessage_ConvertsTextPart()
    {
        // Arrange
        var message = CreateAssistantMessage("id", "session", "parent-id");
        var messageWithParts = new MessageWithParts
        {
            Message = message,
            Parts = new List<Part> { CreateTextPart("Hello, world!") }
        };

        // Act
        var chatMessage = MessageConverter.ToChatMessage(messageWithParts);

        // Assert
        chatMessage.Contents.Should().HaveCount(1);
        chatMessage.Contents[0].Should().BeOfType<TextContent>();
        ((TextContent)chatMessage.Contents[0]).Text.Should().Be("Hello, world!");
    }

    [Fact]
    public void ToChatMessage_ConvertsToolPart()
    {
        // Arrange
        var message = CreateAssistantMessage("id", "session", "parent-id");
        var toolPart = CreateToolPart("tool-123", "searchFiles");
        var messageWithParts = new MessageWithParts
        {
            Message = message,
            Parts = new List<Part> { toolPart }
        };

        // Act
        var chatMessage = MessageConverter.ToChatMessage(messageWithParts);

        // Assert
        chatMessage.Contents.Should().HaveCount(1);
        chatMessage.Contents[0].Should().BeOfType<FunctionResultContent>();
        var functionResult = (FunctionResultContent)chatMessage.Contents[0];
        functionResult.CallId.Should().Be("tool-123");
    }

    [Fact]
    public void ToPartInputs_WithNullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToPartInputs(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public void ToPartInputs_ConvertsTextContent()
    {
        // Arrange
        var chatMessage = new ChatMessage(ChatRole.User, "Hello!");

        // Act
        var parts = MessageConverter.ToPartInputs(chatMessage);

        // Assert
        parts.Should().HaveCount(1);
        parts[0].Should().BeOfType<PartInput>();
        parts[0].Type.Should().Be("text");
        parts[0].Text.Should().Be("Hello!");
    }

    [Fact]
    public void ToChatResponse_WithNullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => MessageConverter.ToChatResponse(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("messageWithParts");
    }

    [Fact]
    public void ToChatResponse_SetsResponseIdAndCreatedAt()
    {
        // Arrange
        var createdAtMs = 1718459400000L; // 2024-06-15T14:30:00Z
        var message = CreateAssistantMessage("response-123", "session", "parent-id", createdAtMs);
        var messageWithParts = new MessageWithParts
        {
            Message = message,
            Parts = new List<Part> { CreateTextPart("Response") }
        };

        // Act
        var response = MessageConverter.ToChatResponse(messageWithParts);

        // Assert
        response.ResponseId.Should().Be("response-123");
        response.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(createdAtMs));
        response.Messages.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("user")]
    [InlineData("assistant")]
    [InlineData("system")]
    [InlineData("tool")]
    public void ToChatRole_ConvertsCorrectly(string messageRole)
    {
        // Act
        var chatRole = MessageConverter.ToChatRole(messageRole);

        // Assert
        var expectedRole = messageRole.ToLowerInvariant() switch
        {
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.Assistant
        };
        chatRole.Should().Be(expectedRole);
    }

    [Fact]
    public void ToMessageRole_ConvertsUserRole()
    {
        MessageConverter.ToMessageRole(ChatRole.User).Should().Be("user");
    }

    [Fact]
    public void ToMessageRole_ConvertsAssistantRole()
    {
        MessageConverter.ToMessageRole(ChatRole.Assistant).Should().Be("assistant");
    }

    [Fact]
    public void ToMessageRole_ConvertsSystemRole()
    {
        MessageConverter.ToMessageRole(ChatRole.System).Should().Be("system");
    }

    [Fact]
    public void ToMessageRole_ConvertsToolRole()
    {
        MessageConverter.ToMessageRole(ChatRole.Tool).Should().Be("tool");
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
            new MessageWithParts
            {
                Message = CreateUserMessage("1", "session", "user"),
                Parts = new List<Part> { CreateTextPart("Hello") }
            },
            new MessageWithParts
            {
                Message = CreateAssistantMessage("2", "session", "1"),
                Parts = new List<Part> { CreateTextPart("Hi there!") }
            }
        };

        // Act
        var chatMessages = MessageConverter.ToChatMessages(messages);

        // Assert
        chatMessages.Should().HaveCount(2);
        chatMessages[0].Role.Should().Be(ChatRole.User);
        chatMessages[1].Role.Should().Be(ChatRole.Assistant);
    }

    // Helper methods to create test data
    private static UserMessage CreateUserMessage(string id, string sessionId, string role)
    {
        return new UserMessage
        {
            Id = id,
            SessionId = sessionId,
            Role = role,
            Time = new MessageTime { Created = TestDate.ToUnixTimeMilliseconds() },
            Agent = "test-agent",
            Model = new ModelReference { ProviderId = "test-provider", ModelId = "test-model" }
        };
    }

    private static AssistantMessage CreateAssistantMessage(string id, string sessionId, string parentId, long? createdMs = null)
    {
        return new AssistantMessage
        {
            Id = id,
            SessionId = sessionId,
            Role = "assistant",
            Time = new MessageTime { Created = createdMs ?? TestDate.ToUnixTimeMilliseconds() },
            ParentId = parentId,
            ModelId = "test-model",
            ProviderId = "test-provider",
            Mode = "normal",
            Path = new MessagePath { Cwd = "/test", Root = "/test" },
            Cost = 0.0,
            Tokens = new TokenUsage
            {
                Input = 10,
                Output = 20,
                Reasoning = 0,
                Cache = new CacheUsage { Read = 0, Write = 0 }
            }
        };
    }

    private static Part CreateTextPart(string text)
    {
        return new Part
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = "test-session",
            MessageId = "test-message",
            Type = "text",
            Text = text
        };
    }

    private static Part CreateToolPart(string callId, string toolName)
    {
        return new Part
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = "test-session",
            MessageId = "test-message",
            Type = "tool",
            CallId = callId,
            Tool = toolName,
            State = "completed",
            Output = "result"
        };
    }
}
