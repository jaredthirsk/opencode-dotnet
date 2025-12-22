using LionFire.OpenCode.Serve;

namespace LionFire.OpenCode.Serve.Tests;

public class OpenCodeClientOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new OpenCodeClientOptions();

        options.BaseUrl.Should().Be("http://localhost:9123");
        options.Directory.Should().BeNull();
        options.DefaultTimeout.Should().Be(TimeSpan.FromSeconds(30));
        options.MessageTimeout.Should().Be(TimeSpan.FromMinutes(5));
        options.EnableRetry.Should().BeTrue();
        options.MaxRetryAttempts.Should().Be(3);
        options.RetryDelaySeconds.Should().Be(2);
    }

    [Theory]
    [InlineData("http://localhost:9123")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://localhost:9123")]
    [InlineData("http://127.0.0.1:9123")]
    public void ValidateBaseUrl_ValidUrls_ReturnsTrue(string url)
    {
        var options = new OpenCodeClientOptions { BaseUrl = url };

        options.ValidateBaseUrl().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://localhost:9123")]
    [InlineData("localhost:9123")]
    public void ValidateBaseUrl_InvalidUrls_ReturnsFalse(string url)
    {
        var options = new OpenCodeClientOptions { BaseUrl = url };

        options.ValidateBaseUrl().Should().BeFalse();
    }

    [Fact]
    public void Validator_ValidOptions_ReturnsSuccess()
    {
        var options = new OpenCodeClientOptions();
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validator_EmptyBaseUrl_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { BaseUrl = "" };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("BaseUrl"));
    }

    [Fact]
    public void Validator_InvalidBaseUrl_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { BaseUrl = "not-a-url" };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validator_NegativeTimeout_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { DefaultTimeout = TimeSpan.FromSeconds(-1) };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("DefaultTimeout"));
    }

    [Fact]
    public void Validator_NegativeMessageTimeout_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { MessageTimeout = TimeSpan.FromSeconds(-1) };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validator_NegativeRetryAttempts_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { MaxRetryAttempts = -1 };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validator_ZeroRetryAttempts_ReturnsSuccess()
    {
        var options = new OpenCodeClientOptions { MaxRetryAttempts = 0 };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    #region Enhanced Validation Tests

    [Fact]
    public void Validator_BaseUrlWithPath_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { BaseUrl = "http://localhost:9123/api/v1" };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("should not contain a path"));
    }

    [Fact]
    public void Validator_HttpSchemeWithPort443_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { BaseUrl = "http://localhost:443" };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("port 443") || f.Contains("HTTPS"));
    }

    [Fact]
    public void Validator_ExcessiveTimeout_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { DefaultTimeout = TimeSpan.FromHours(48) };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("exceeds maximum reasonable value"));
    }

    [Fact]
    public void Validator_ExcessiveMessageTimeout_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { MessageTimeout = TimeSpan.FromDays(2) };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("MessageTimeout") && f.Contains("exceeds"));
    }

    [Fact]
    public void Validator_ExcessiveRetryAttempts_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { MaxRetryAttempts = 50 };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("MaxRetryAttempts") && f.Contains("exceeds"));
    }

    [Fact]
    public void Validator_ExcessiveRetryDelay_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { RetryDelaySeconds = 600 };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("RetryDelaySeconds") && f.Contains("exceeds"));
    }

    [Fact]
    public void Validator_ExcessiveCircuitBreakerThreshold_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { CircuitBreakerThreshold = 200 };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("CircuitBreakerThreshold") && f.Contains("exceeds"));
    }

    [Fact]
    public void Validator_ExcessiveCircuitBreakerDuration_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { CircuitBreakerDuration = TimeSpan.FromHours(1) };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("CircuitBreakerDuration") && f.Contains("exceeds"));
    }

    [Fact]
    public void Validator_ExcessiveJitter_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { MaxRetryJitter = TimeSpan.FromMinutes(2) };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("MaxRetryJitter") && f.Contains("exceeds"));
    }

    [Fact]
    public void Validator_DirectoryWithUrl_ReturnsFail()
    {
        var options = new OpenCodeClientOptions { Directory = "http://example.com/path" };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("URL") && f.Contains("file system path"));
    }

    [Fact]
    public void Validator_ValidDirectoryPath_ReturnsSuccess()
    {
        var options = new OpenCodeClientOptions { Directory = "/home/user/project" };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validator_AllValidOptions_ReturnsSuccess()
    {
        var options = new OpenCodeClientOptions
        {
            BaseUrl = "https://api.example.com:9123",
            Directory = "/home/user/project",
            DefaultTimeout = TimeSpan.FromMinutes(1),
            MessageTimeout = TimeSpan.FromMinutes(10),
            MaxRetryAttempts = 5,
            RetryDelaySeconds = 3,
            CircuitBreakerThreshold = 10,
            CircuitBreakerDuration = TimeSpan.FromSeconds(30),
            MaxRetryJitter = TimeSpan.FromSeconds(2)
        };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validator_MultipleErrors_ReportsAll()
    {
        var options = new OpenCodeClientOptions
        {
            BaseUrl = "",
            DefaultTimeout = TimeSpan.Zero,
            MaxRetryAttempts = -1
        };
        var validator = new OpenCodeClientOptionsValidator();

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Count().Should().BeGreaterThanOrEqualTo(3);
    }

    #endregion
}
