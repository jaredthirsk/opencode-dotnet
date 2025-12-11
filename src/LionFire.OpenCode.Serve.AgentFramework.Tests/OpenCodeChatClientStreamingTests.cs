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
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);
        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
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
        var session = new Session("stream-session-id", TestDate, TestDate, SessionStatus.Active);

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _mockClient.SendMessageStreamingAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateStreamingResponse());

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
        await _mockClient.Received(1).CreateSessionAsync(null, Arg.Any<CancellationToken>());
        chatClient.SessionId.Should().Be("stream-session-id");
    }

    [Fact]
    public async Task GetStreamingResponseAsync_YieldsUpdatesWithDelta()
    {
        // Arrange
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _mockClient.SendMessageStreamingAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateStreamingResponse(
                new MessageUpdate { Delta = "Hello", MessageId = "msg-1" },
                new MessageUpdate { Delta = " world", MessageId = "msg-1" },
                new MessageUpdate { Done = true, MessageId = "msg-1" }));

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
        updates.Should().HaveCount(3);
        updates[0].Text.Should().Be("Hello");
        updates[0].Role.Should().Be(ChatRole.Assistant);
        updates[1].Text.Should().Be(" world");
        updates[2].FinishReason.Should().Be(ChatFinishReason.Stop);
    }

    [Fact]
    public async Task GetStreamingResponseAsync_SkipsNullDeltas()
    {
        // Arrange
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _mockClient.SendMessageStreamingAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateStreamingResponse(
                new MessageUpdate { Delta = "Text", MessageId = "msg-1" },
                new MessageUpdate { Delta = null, MessageId = "msg-1" }, // This should be skipped for content
                new MessageUpdate { Done = true, MessageId = "msg-1" }));

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
        updates.Should().HaveCount(2); // Text delta + done
    }

    [Fact]
    public async Task GetStreamingResponseAsync_SetsMessageIdOnUpdates()
    {
        // Arrange
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _mockClient.SendMessageStreamingAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateStreamingResponse(
                new MessageUpdate { Delta = "Content", MessageId = "unique-msg-id" },
                new MessageUpdate { Done = true, MessageId = "unique-msg-id" }));

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
        var session = new Session("test-session", TestDate, TestDate, SessionStatus.Active);
        var cts = new CancellationTokenSource();

        _mockClient.CreateSessionAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(session);

        async IAsyncEnumerable<MessageUpdate> InfiniteStream([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            int i = 0;
            while (!ct.IsCancellationRequested)
            {
                yield return new MessageUpdate { Delta = $"Part {i++}", MessageId = "msg-1" };
                await Task.Delay(10, ct);
            }
        }

        _mockClient.SendMessageStreamingAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<MessagePart>>(),
            Arg.Any<CancellationToken>())
            .Returns(call => InfiniteStream(call.Arg<CancellationToken>()));

        var chatClient = new OpenCodeChatClient(_mockClient);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Infinite test")
        };

        // Act
        var updates = new List<ChatResponseUpdate>();
        var consumeTask = Task.Run(async () =>
        {
            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
            {
                updates.Add(update);
                if (updates.Count >= 3)
                {
                    await cts.CancelAsync();
                }
            }
        });

        // Assert
        await Task.WhenAny(consumeTask, Task.Delay(TimeSpan.FromSeconds(5)));
        updates.Count.Should().BeGreaterOrEqualTo(3);
    }

    private static async IAsyncEnumerable<MessageUpdate> CreateStreamingResponse(params MessageUpdate[] updates)
    {
        foreach (var update in updates)
        {
            yield return update;
        }
        await Task.CompletedTask;
    }
}
