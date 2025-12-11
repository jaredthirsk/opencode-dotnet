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
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);
        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
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

    [Fact]
    public async Task GetResponseAsync_WithoutSession_CreatesNewSession()
    {
        // Arrange
        var session = new Session("test-session-id", TestDate, TestDate, SessionStatus.Active);
        var responseMessage = new Message(
            "msg-1", "test-session-id", MessageRole.Assistant,
            new[] { new TextPart("Hello!") }, TestDate);

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _mockClient.SendMessageAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
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
        await _mockClient.Received(1).CreateSessionAsync(null, Arg.Any<CancellationToken>());
        chatClient.SessionId.Should().Be("test-session-id");
    }

    [Fact]
    public async Task GetResponseAsync_WithExistingSession_ReusesSession()
    {
        // Arrange
        var responseMessage = new Message(
            "msg-1", "existing-session", MessageRole.Assistant,
            new[] { new TextPart("Hello!") }, TestDate);

        _mockClient.SendMessageAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
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
        await _mockClient.DidNotReceive().CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _mockClient.Received(1).SendMessageAsync("existing-session", Arg.Any<IReadOnlyList<MessagePart>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResponseAsync_ConvertsResponseCorrectly()
    {
        // Arrange
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);
        var responseMessage = new Message(
            "msg-123", "test-session", MessageRole.Assistant,
            new[] { new TextPart("The answer is 42.") },
            DateTimeOffset.Parse("2024-01-15T10:30:00Z"));

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _mockClient.SendMessageAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
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
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);
        var responseMessage = new Message(
            "msg-1", "test-session", MessageRole.Assistant,
            new[] { new TextPart("Response") }, TestDate);

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _mockClient.SendMessageAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
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
        await _mockClient.Received(1).SendMessageAsync(
            Arg.Any<string>(),
            Arg.Is<IReadOnlyList<MessagePart>>(parts =>
                parts.Count == 1 &&
                parts[0] is TextPart),
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
