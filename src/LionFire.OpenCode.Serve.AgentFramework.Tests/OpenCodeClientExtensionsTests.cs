using FluentAssertions;
using Microsoft.Extensions.AI;
using NSubstitute;
using Xunit;
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.AgentFramework;

namespace LionFire.OpenCode.Serve.AgentFramework.Tests;

public class OpenCodeClientExtensionsTests
{
    private readonly IOpenCodeClient _mockClient;

    public OpenCodeClientExtensionsTests()
    {
        _mockClient = Substitute.For<IOpenCodeClient>();
    }

    [Fact]
    public void AsChatClient_ReturnsOpenCodeChatClient()
    {
        // Act
        var chatClient = _mockClient.AsChatClient();

        // Assert
        chatClient.Should().NotBeNull();
        chatClient.Should().BeOfType<OpenCodeChatClient>();
    }

    [Fact]
    public void AsChatClient_WithOptions_AppliesOptions()
    {
        // Arrange
        var options = new OpenCodeChatClientOptions
        {
            ModelId = "test-model",
            BaseUrl = "http://test:9999"
        };

        // Act
        var chatClient = _mockClient.AsChatClient(options) as OpenCodeChatClient;

        // Assert
        chatClient!.Metadata.ProviderUri.Should().Be(new Uri("http://test:9999"));
    }

    [Fact]
    public void AsChatClient_WithSessionId_SetsSessionId()
    {
        // Act
        var chatClient = _mockClient.AsChatClient("existing-session-123");

        // Assert
        var openCodeChatClient = chatClient.Should().BeOfType<OpenCodeChatClient>().Subject;
        openCodeChatClient.SessionId.Should().Be("existing-session-123");
    }

    [Fact]
    public void AsChatClient_WithSessionIdAndOptions_AppliesBoth()
    {
        // Arrange
        var options = new OpenCodeChatClientOptions
        {
            ModelId = "custom-model"
        };

        // Act
        var chatClient = _mockClient.AsChatClient("session-456", options);

        // Assert
        var openCodeChatClient = chatClient.Should().BeOfType<OpenCodeChatClient>().Subject;
        openCodeChatClient.SessionId.Should().Be("session-456");
    }

    [Fact]
    public void AsChatClient_ReturnsIChatClientInterface()
    {
        // Act
        IChatClient chatClient = _mockClient.AsChatClient();

        // Assert
        chatClient.Should().NotBeNull();
        chatClient.Should().BeAssignableTo<IChatClient>();
    }

    [Fact]
    public void AsChatClient_AllowsFluentChaining()
    {
        // Act - Verify fluent API doesn't throw
        var chatClient = _mockClient
            .AsChatClient(new OpenCodeChatClientOptions
            {
                ModelId = "chained-model",
                Directory = "/chained/dir"
            }) as OpenCodeChatClient;

        // Assert
        chatClient.Should().NotBeNull();
    }

    [Fact]
    public void AsChatClient_UnderlyingClientAccessible()
    {
        // Act
        var chatClient = _mockClient.AsChatClient();
        var resolvedClient = chatClient.GetService(typeof(IOpenCodeClient));

        // Assert
        resolvedClient.Should().BeSameAs(_mockClient);
    }
}
