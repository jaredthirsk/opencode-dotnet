using FluentAssertions;
using Microsoft.Extensions.AI;
using NSubstitute;
using Xunit;
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.AgentFramework;

namespace LionFire.OpenCode.Serve.AgentFramework.Tests;

public class OpenCodeChatClientTests
{
    private readonly IOpenCodeClient _mockClient;
    private static readonly DateTimeOffset TestDate = DateTimeOffset.Parse("2024-01-15T10:00:00Z");

    public OpenCodeChatClientTests()
    {
        _mockClient = Substitute.For<IOpenCodeClient>();
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new OpenCodeChatClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("client");
    }

    [Fact]
    public void Constructor_WithValidClient_SetsMetadata()
    {
        // Act
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Assert
        chatClient.Metadata.Should().NotBeNull();
        chatClient.Metadata.ProviderName.Should().Be("OpenCode");
    }

    [Fact]
    public void Constructor_WithCustomOptions_UsesOptions()
    {
        // Arrange
        var options = new OpenCodeChatClientOptions
        {
            ModelId = "custom-model",
            BaseUrl = "http://custom:8080"
        };

        // Act
        var chatClient = new OpenCodeChatClient(_mockClient, options);

        // Assert
        chatClient.Metadata.ProviderUri.Should().Be(new Uri("http://custom:8080"));
    }

    [Fact]
    public void SessionId_CanBeSetAndRetrieved()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act
        chatClient.SessionId = "test-session-123";

        // Assert
        chatClient.SessionId.Should().Be("test-session-123");
    }

    [Fact]
    public async Task GetResponseAsync_WithNullMessages_ThrowsArgumentNullException()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act & Assert
        var act = () => chatClient.GetResponseAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("chatMessages");
    }

    [Fact]
    public async Task GetResponseAsync_WithNoUserMessage_ThrowsArgumentException()
    {
        // Arrange
        var session = CreateSession("test-session", TestDate);
        _mockClient.CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant")
        };

        // Act & Assert
        var act = () => chatClient.GetResponseAsync(messages);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*No user message found*");
    }

    private static Session CreateSession(string sessionId, DateTimeOffset time)
    {
        return new Session
        {
            Id = sessionId,
            ProjectId = "test-project",
            Directory = "/test",
            Title = "Test Session",
            Version = "1.0",
            Time = new SessionTime
            {
                Created = time.ToUnixTimeMilliseconds(),
                Updated = time.ToUnixTimeMilliseconds()
            }
        };
    }

    [Fact]
    public async Task GetResponseAsync_WithoutSession_CreatesNewSession()
    {
        // Arrange
        var session = CreateSession("test-session-id", TestDate);
        var responseMessage = CreateAssistantMessageWithParts(
            "msg-1", "test-session-id", "Hello!", TestDate);

        _mockClient.CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _mockClient.PromptAsync(
            Arg.Any<string>(),
            Arg.Any<SendMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(responseMessage);

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };

        // Act
        await chatClient.GetResponseAsync(messages);

        // Assert
        await _mockClient.Received(1).CreateSessionAsync(null, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        chatClient.SessionId.Should().Be("test-session-id");
    }

    private static MessageWithParts CreateAssistantMessageWithParts(
        string messageId, string sessionId, string text, DateTimeOffset time)
    {
        var message = new AssistantMessage
        {
            Id = messageId,
            SessionId = sessionId,
            Role = "assistant",
            Time = new MessageTime
            {
                Created = time.ToUnixTimeMilliseconds()
            },
            ParentId = "parent-msg",
            ModelId = "test-model",
            ProviderId = "test-provider",
            Mode = "normal",
            Path = new MessagePath
            {
                Cwd = "/test",
                Root = "/test"
            },
            Cost = 0.0,
            Tokens = new TokenUsage
            {
                Input = 10,
                Output = 20,
                Reasoning = 0,
                Cache = new CacheUsage
                {
                    Read = 0,
                    Write = 0
                }
            }
        };

        var parts = new List<Part>
        {
            new Part
            {
                Id = "part-1",
                SessionId = sessionId,
                MessageId = messageId,
                Type = "text",
                Text = text
            }
        };

        return new MessageWithParts
        {
            Message = message,
            Parts = parts
        };
    }

    [Fact]
    public async Task GetResponseAsync_WithExistingSession_ReusesSession()
    {
        // Arrange
        var responseMessage = CreateAssistantMessageWithParts(
            "msg-1", "existing-session", "Hello!", TestDate);

        _mockClient.PromptAsync(
            Arg.Any<string>(),
            Arg.Any<SendMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(responseMessage);

        var chatClient = new OpenCodeChatClient(_mockClient)
        {
            SessionId = "existing-session"
        };
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };

        // Act
        await chatClient.GetResponseAsync(messages);

        // Assert
        await _mockClient.DidNotReceive().CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockClient.Received(1).PromptAsync("existing-session", Arg.Any<SendMessageRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResponseAsync_ConvertsResponseCorrectly()
    {
        // Arrange
        var session = CreateSession("test-session", TestDate);
        var responseTime = DateTimeOffset.Parse("2024-01-15T10:30:00Z");
        var responseMessage = CreateAssistantMessageWithParts(
            "msg-123", "test-session", "The answer is 42.", responseTime);

        _mockClient.CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _mockClient.PromptAsync(
            Arg.Any<string>(),
            Arg.Any<SendMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(responseMessage);

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "What is the answer?")
        };

        // Act
        var response = await chatClient.GetResponseAsync(messages);

        // Assert
        response.Should().NotBeNull();
        response.ResponseId.Should().Be("msg-123");
        response.Messages.Should().HaveCount(1);
        response.Messages[0].Role.Should().Be(ChatRole.Assistant);
        response.Messages[0].Text.Should().Be("The answer is 42.");
    }

    [Fact]
    public async Task GetResponseAsync_WithTextContent_ConvertsToTextPart()
    {
        // Arrange
        var session = CreateSession("test-session", TestDate);
        var responseMessage = CreateAssistantMessageWithParts(
            "msg-1", "test-session", "Response", TestDate);

        _mockClient.CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _mockClient.PromptAsync(
            Arg.Any<string>(),
            Arg.Any<SendMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(responseMessage);

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Test message")
        };

        // Act
        await chatClient.GetResponseAsync(messages);

        // Assert
        await _mockClient.Received(1).PromptAsync(
            Arg.Any<string>(),
            Arg.Is<SendMessageRequest>(req =>
                req.Parts.Count == 1 &&
                req.Parts[0].Type == "text"),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetService_ReturnsOpenCodeClient()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act
        var result = chatClient.GetService(typeof(IOpenCodeClient));

        // Assert
        result.Should().BeSameAs(_mockClient);
    }

    [Fact]
    public void GetService_ReturnsSelf()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act
        var result = chatClient.GetService(typeof(OpenCodeChatClient));

        // Assert
        result.Should().BeSameAs(chatClient);
    }

    [Fact]
    public void GetService_WithKey_ReturnsNull()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act
        var result = chatClient.GetService(typeof(IOpenCodeClient), "some-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetService_WithUnknownType_ReturnsNull()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act
        var result = chatClient.GetService(typeof(string));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act & Assert
        var act = () => chatClient.Dispose();
        act.Should().NotThrow();
    }
}
