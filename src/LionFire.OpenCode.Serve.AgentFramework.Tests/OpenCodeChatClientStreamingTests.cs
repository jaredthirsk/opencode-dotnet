using FluentAssertions;
using Microsoft.Extensions.AI;
using NSubstitute;
using Xunit;
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.AgentFramework;

namespace LionFire.OpenCode.Serve.AgentFramework.Tests;

public class OpenCodeChatClientStreamingTests
{
    private readonly IOpenCodeClient _mockClient;
    private static readonly DateTimeOffset TestDate = DateTimeOffset.Parse("2024-01-15T10:00:00Z");

    public OpenCodeChatClientStreamingTests()
    {
        _mockClient = Substitute.For<IOpenCodeClient>();
    }

    [Fact]
    public async Task GetStreamingResponseAsync_WithNullMessages_ThrowsArgumentNullException()
    {
        // Arrange
        var chatClient = new OpenCodeChatClient(_mockClient);

        // Act & Assert
        var act = async () =>
        {
            await foreach (var _ in chatClient.GetStreamingResponseAsync(null!))
            {
                // consume
            }
        };
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("chatMessages");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_WithNoUserMessage_ThrowsArgumentException()
    {
        // Arrange
        var session = CreateSession("test-session");
        _mockClient.CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant")
        };

        // Act & Assert
        var act = async () =>
        {
            await foreach (var _ in chatClient.GetStreamingResponseAsync(messages))
            {
                // consume
            }
        };
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*No user message found*");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_WithoutSession_CreatesNewSession()
    {
        // Arrange
        var session = CreateSession("stream-session-id");
        var responseMessage = CreateMessageWithParts("response-id", "stream-session-id", "Hello!");

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
        await foreach (var _ in chatClient.GetStreamingResponseAsync(messages))
        {
            // consume
        }

        // Assert
        await _mockClient.Received(1).CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        chatClient.SessionId.Should().Be("stream-session-id");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_YieldsSingleUpdate()
    {
        // Arrange
        var session = CreateSession("test-session");
        var responseMessage = CreateMessageWithParts("msg-1", "test-session", "Hello world");

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
            new(ChatRole.User, "Say hello world")
        };

        // Act
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        // Assert
        updates.Should().HaveCount(1);
        updates[0].Role.Should().Be(ChatRole.Assistant);
        updates[0].MessageId.Should().Be("msg-1");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_SetsMessageIdOnUpdate()
    {
        // Arrange
        var session = CreateSession("test-session");
        var responseMessage = CreateMessageWithParts("unique-msg-id", "test-session", "Content");

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
            new(ChatRole.User, "Test")
        };

        // Act
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        // Assert
        updates.Should().OnlyContain(u => u.MessageId == "unique-msg-id");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_WithCancellation_StopsEnumeration()
    {
        // Arrange
        var session = CreateSession("test-session");
        var cts = new CancellationTokenSource();

        _mockClient.CreateSessionAsync(Arg.Any<CreateSessionRequest?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _mockClient.PromptAsync(
            Arg.Any<string>(),
            Arg.Any<SendMessageRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var token = call.Arg<CancellationToken>();
                await Task.Delay(100, token);
                return CreateMessageWithParts("msg-1", "test-session", "Response");
            });

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Test")
        };

        // Act
        await cts.CancelAsync();
        var act = async () =>
        {
            await foreach (var _ in chatClient.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
            {
                // consume
            }
        };

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // Helper methods to create test data
    private static Session CreateSession(string id)
    {
        return new Session
        {
            Id = id,
            ProjectId = "test-project",
            Directory = "/test",
            Title = "Test Session",
            Version = "1",
            Time = new SessionTime
            {
                Created = TestDate.ToUnixTimeMilliseconds(),
                Updated = TestDate.ToUnixTimeMilliseconds()
            }
        };
    }

    private static MessageWithParts CreateMessageWithParts(string messageId, string sessionId, string text)
    {
        var message = new AssistantMessage
        {
            Id = messageId,
            SessionId = sessionId,
            Role = "assistant",
            Time = new MessageTime { Created = TestDate.ToUnixTimeMilliseconds() },
            ParentId = "parent-id",
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

        var textPart = new Part
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionId,
            MessageId = messageId,
            Type = "text",
            Text = text
        };

        return new MessageWithParts
        {
            Message = message,
            Parts = new List<Part> { textPart }
        };
    }
}
