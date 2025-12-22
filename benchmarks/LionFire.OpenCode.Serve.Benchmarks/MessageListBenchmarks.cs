using System.Text.Json;
using BenchmarkDotNet.Attributes;
using LionFire.OpenCode.Serve.Internal;
using LionFire.OpenCode.Serve.Models;

namespace LionFire.OpenCode.Serve.Benchmarks;

/// <summary>
/// Benchmarks for message list operations and thread serialization.
/// Target: &lt;10ms for 100 messages serialization.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class MessageListBenchmarks
{
    private List<MessageWithParts> _messagesSmall = null!;  // 10 messages
    private List<MessageWithParts> _messagesMedium = null!; // 100 messages
    private List<MessageWithParts> _messagesLarge = null!;  // 500 messages

    private string _messagesSmallJson = null!;
    private string _messagesMediumJson = null!;
    private string _messagesLargeJson = null!;

    private JsonSerializerOptions _options = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = JsonOptions.SourceGenerated;

        _messagesSmall = GenerateMessages(10);
        _messagesMedium = GenerateMessages(100);
        _messagesLarge = GenerateMessages(500);

        _messagesSmallJson = JsonSerializer.Serialize(_messagesSmall, _options);
        _messagesMediumJson = JsonSerializer.Serialize(_messagesMedium, _options);
        _messagesLargeJson = JsonSerializer.Serialize(_messagesLarge, _options);
    }

    private static List<MessageWithParts> GenerateMessages(int count)
    {
        var messages = new List<MessageWithParts>(count);
        var baseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        for (int i = 0; i < count; i++)
        {
            bool isUser = i % 2 == 0;
            var messageId = $"msg_{i:D6}";
            var sessionId = "ses_benchmark123";

            var message = new MessageWithParts
            {
                Message = new Message
                {
                    Id = messageId,
                    SessionId = sessionId,
                    Role = isUser ? "user" : "assistant",
                    Time = new MessageTime
                    {
                        Created = baseTime + (i * 1000),
                        Completed = baseTime + (i * 1000) + 500
                    },
                    ModelId = isUser ? null : "claude-3-opus-20240229",
                    ProviderId = isUser ? null : "anthropic",
                    Tokens = isUser ? null : new TokenUsage
                    {
                        Input = 1000 + i,
                        Output = 500 + i,
                        Reasoning = 100
                    },
                    Cost = isUser ? null : 0.01 + (i * 0.001)
                },
                Parts = GenerateParts(messageId, sessionId, isUser, i)
            };

            messages.Add(message);
        }

        return messages;
    }

    private static List<Part> GenerateParts(string messageId, string sessionId, bool isUser, int index)
    {
        var parts = new List<Part>();

        if (isUser)
        {
            // User message: just text
            parts.Add(new Part
            {
                Id = $"prt_{index:D6}_text",
                SessionId = sessionId,
                MessageId = messageId,
                Type = "text",
                Text = $"This is user message {index}. Please help me with my coding task. I need to implement a feature that processes data efficiently."
            });
        }
        else
        {
            // Assistant message: text + possibly tool calls
            parts.Add(new Part
            {
                Id = $"prt_{index:D6}_text",
                SessionId = sessionId,
                MessageId = messageId,
                Type = "text",
                Text = $"I'll help you with that task. Here's my response for message {index}. Let me analyze your requirements and provide a solution.",
                Time = new PartTime
                {
                    Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    End = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 100
                }
            });

            // Add tool call for some messages
            if (index % 4 == 1)
            {
                parts.Add(new Part
                {
                    Id = $"prt_{index:D6}_tool",
                    SessionId = sessionId,
                    MessageId = messageId,
                    Type = "tool",
                    Tool = "Read",
                    CallId = $"call_{index:D6}",
                    State = "completed",
                    Input = new Dictionary<string, object> { ["file_path"] = "/src/example.cs" },
                    Output = "// File content here"
                });
            }
        }

        return parts;
    }

    #region Serialization Benchmarks

    [Benchmark(Description = "Serialize: 10 messages")]
    public string Serialize10Messages()
    {
        return JsonSerializer.Serialize(_messagesSmall, _options);
    }

    [Benchmark(Description = "Serialize: 100 messages")]
    public string Serialize100Messages()
    {
        return JsonSerializer.Serialize(_messagesMedium, _options);
    }

    [Benchmark(Description = "Serialize: 500 messages")]
    public string Serialize500Messages()
    {
        return JsonSerializer.Serialize(_messagesLarge, _options);
    }

    #endregion

    #region Deserialization Benchmarks

    [Benchmark(Description = "Deserialize: 10 messages")]
    public List<MessageWithParts>? Deserialize10Messages()
    {
        return JsonSerializer.Deserialize<List<MessageWithParts>>(_messagesSmallJson, _options);
    }

    [Benchmark(Description = "Deserialize: 100 messages")]
    public List<MessageWithParts>? Deserialize100Messages()
    {
        return JsonSerializer.Deserialize<List<MessageWithParts>>(_messagesMediumJson, _options);
    }

    [Benchmark(Description = "Deserialize: 500 messages")]
    public List<MessageWithParts>? Deserialize500Messages()
    {
        return JsonSerializer.Deserialize<List<MessageWithParts>>(_messagesLargeJson, _options);
    }

    #endregion

    #region Message List Operations

    [Benchmark(Description = "Filter: Text parts only (100 msgs)")]
    public List<Part> FilterTextParts()
    {
        return _messagesMedium
            .SelectMany(m => m.Parts ?? Enumerable.Empty<Part>())
            .Where(p => p.IsTextPart)
            .ToList();
    }

    [Benchmark(Description = "Filter: Tool parts only (100 msgs)")]
    public List<Part> FilterToolParts()
    {
        return _messagesMedium
            .SelectMany(m => m.Parts ?? Enumerable.Empty<Part>())
            .Where(p => p.IsToolPart)
            .ToList();
    }

    [Benchmark(Description = "Aggregate: Total tokens (100 msgs)")]
    public (int Input, int Output) AggregateTokens()
    {
        var input = 0;
        var output = 0;

        foreach (var msg in _messagesMedium)
        {
            if (msg.Message?.Tokens != null)
            {
                input += msg.Message.Tokens.Input;
                output += msg.Message.Tokens.Output;
            }
        }

        return (input, output);
    }

    [Benchmark(Description = "Aggregate: Total cost (100 msgs)")]
    public double AggregateCost()
    {
        double total = 0;

        foreach (var msg in _messagesMedium)
        {
            if (msg.Message?.Cost.HasValue == true)
            {
                total += msg.Message.Cost.Value;
            }
        }

        return total;
    }

    [Benchmark(Description = "Search: Find by messageId (100 msgs)")]
    public MessageWithParts? FindByMessageId()
    {
        return _messagesMedium.FirstOrDefault(m => m.Message?.Id == "msg_000050");
    }

    [Benchmark(Description = "Search: Find by role (100 msgs)")]
    public List<MessageWithParts> FindByRole()
    {
        return _messagesMedium.Where(m => m.Message?.Role == "assistant").ToList();
    }

    #endregion

    #region Roundtrip Benchmarks

    [Benchmark(Description = "Roundtrip: 100 messages")]
    public List<MessageWithParts>? Roundtrip100Messages()
    {
        var json = JsonSerializer.Serialize(_messagesMedium, _options);
        return JsonSerializer.Deserialize<List<MessageWithParts>>(json, _options);
    }

    #endregion
}
