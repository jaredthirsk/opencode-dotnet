using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.AgentFramework;

namespace LionFire.OpenCode.Serve.AgentFramework.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenCodeChatClient_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => LionFire.OpenCode.Serve.AgentFramework.ServiceCollectionExtensions.AddOpenCodeChatClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddOpenCodeChatClient_RegistersIChatClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddOpenCodeChatClient();
        var provider = services.BuildServiceProvider();

        // Assert
        var chatClient = provider.GetService<IChatClient>();
        chatClient.Should().NotBeNull();
        chatClient.Should().BeOfType<OpenCodeChatClient>();
    }

    [Fact]
    public void AddOpenCodeChatClient_WithOptions_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddOpenCodeChatClient(options =>
        {
            options.ModelId = "configured-model";
            options.Directory = "/configured/dir";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var chatClient = provider.GetRequiredService<IChatClient>() as OpenCodeChatClient;
        chatClient.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenCodeChatClient_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOpenCodeChatClient();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddOpenCodeChatClient_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddOpenCodeChatClient();
        var provider = services.BuildServiceProvider();

        // Assert
        var client1 = provider.GetRequiredService<IChatClient>();
        var client2 = provider.GetRequiredService<IChatClient>();
        client1.Should().BeSameAs(client2);
    }

    [Fact]
    public void AddOpenCodeChatClient_DoesNotOverrideExisting()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        var existingChatClient = Substitute.For<IChatClient>();
        services.AddSingleton(mockClient);
        services.AddSingleton(existingChatClient); // Register first

        // Act
        services.AddOpenCodeChatClient(); // Should not override due to TryAdd
        var provider = services.BuildServiceProvider();

        // Assert
        var resolvedClient = provider.GetRequiredService<IChatClient>();
        resolvedClient.Should().BeSameAs(existingChatClient);
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => LionFire.OpenCode.Serve.AgentFramework.ServiceCollectionExtensions.AddKeyedOpenCodeChatClient(null!, "key");
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_WithNullKey_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act = () => services.AddKeyedOpenCodeChatClient(null!);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("key");
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act = () => services.AddKeyedOpenCodeChatClient("");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("key");
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_RegistersWithKey()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddKeyedOpenCodeChatClient("opencode-1");
        var provider = services.BuildServiceProvider();

        // Assert
        var chatClient = provider.GetKeyedService<IChatClient>("opencode-1");
        chatClient.Should().NotBeNull();
        chatClient.Should().BeOfType<OpenCodeChatClient>();
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_MultipleKeys_RegistersSeparateInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddKeyedOpenCodeChatClient("key-1", options => options.ModelId = "model-1");
        services.AddKeyedOpenCodeChatClient("key-2", options => options.ModelId = "model-2");
        var provider = services.BuildServiceProvider();

        // Assert
        var client1 = provider.GetKeyedService<IChatClient>("key-1");
        var client2 = provider.GetKeyedService<IChatClient>("key-2");

        client1.Should().NotBeSameAs(client2);
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_WithOptions_AppliesOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddKeyedOpenCodeChatClient("custom-key", options =>
        {
            options.ModelId = "keyed-model";
            options.BaseUrl = "http://keyed:8080";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var chatClient = provider.GetKeyedService<IChatClient>("custom-key") as OpenCodeChatClient;
        chatClient.Should().NotBeNull();
        chatClient!.Metadata.ProviderUri.Should().Be(new Uri("http://keyed:8080"));
    }

    [Fact]
    public void AddKeyedOpenCodeChatClient_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddKeyedOpenCodeChatClient("key");

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void Integration_OpenCodeClientAndChatClient_CanBeRegisteredTogether()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = Substitute.For<IOpenCodeClient>();
        services.AddSingleton(mockClient);

        // Act
        services.AddOpenCodeChatClient(options =>
        {
            options.ModelId = "integrated-model";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var openCodeClient = provider.GetRequiredService<IOpenCodeClient>();
        var chatClient = provider.GetRequiredService<IChatClient>();

        openCodeClient.Should().BeSameAs(mockClient);
        chatClient.Should().NotBeNull();

        // Verify the chat client uses the same underlying client
        var underlyingClient = chatClient.GetService(typeof(IOpenCodeClient));
        underlyingClient.Should().BeSameAs(mockClient);
    }
}
