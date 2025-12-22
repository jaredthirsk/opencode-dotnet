using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using LionFire.OpenCode.Serve.Internal;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.Models.Events;

namespace LionFire.OpenCode.Serve.Benchmarks;

/// <summary>
/// Benchmarks for JSON serialization and deserialization of SDK DTOs.
/// Target: &lt;1ms for single message conversion.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class JsonSerializationBenchmarks
{
    private string _sessionJson = null!;
    private string _messageJson = null!;
    private string _messageWithPartsJson = null!;
    private string _eventJson = null!;
    private string _complexEventJson = null!;

    private Session _session = null!;
    private Message _message = null!;
    private MessageWithParts _messageWithParts = null!;
    private MessagePartUpdatedEvent _event = null!;

    private JsonSerializerOptions _reflectionOptions = null!;
    private JsonSerializerOptions _sourceGenOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _reflectionOptions = JsonOptions.Default;
        _sourceGenOptions = JsonOptions.SourceGenerated;

        // Create test data
        _session = new Session
        {
            Id = "ses_abc123def456",
            ProjectId = "proj_xyz789",
            Directory = "/home/user/projects/myproject",
            Title = "Test Session for Benchmarking",
            Version = "1.0.0",
            Time = new SessionTime
            {
                Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Updated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            },
            Summary = new SessionSummary
            {
                Additions = 150,
                Deletions = 45,
                Files = 12
            }
        };

        _message = new Message
        {
            Id = "msg_test123",
            SessionId = "ses_abc123def456",
            Role = "assistant",
            Time = new MessageTime
            {
                Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Completed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            },
            ModelId = "claude-3-opus-20240229",
            ProviderId = "anthropic",
            Tokens = new TokenUsage
            {
                Input = 1500,
                Output = 2500,
                Reasoning = 500,
                Cache = new CacheUsage { Read = 1000, Write = 200 }
            },
            Cost = 0.025
        };

        _messageWithParts = new MessageWithParts
        {
            Message = _message,
            Parts = new List<Part>
            {
                new Part
                {
                    Id = "prt_001",
                    SessionId = "ses_abc123def456",
                    MessageId = "msg_test123",
                    Type = "text",
                    Text = "Here is my response to your question about benchmarking performance in .NET applications.",
                    Time = new PartTime
                    {
                        Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        End = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                },
                new Part
                {
                    Id = "prt_002",
                    SessionId = "ses_abc123def456",
                    MessageId = "msg_test123",
                    Type = "tool",
                    Tool = "Bash",
                    CallId = "call_xyz",
                    State = "completed",
                    Input = new { command = "dotnet run -c Release" },
                    Output = "Build succeeded."
                }
            }
        };

        _event = new MessagePartUpdatedEvent
        {
            Type = "message.part.updated",
            Properties = new MessagePartUpdatedProperties
            {
                SessionId = "ses_abc123def456",
                MessageId = "msg_test123",
                Part = new Part
                {
                    Id = "prt_001",
                    SessionId = "ses_abc123def456",
                    MessageId = "msg_test123",
                    Type = "text",
                    Text = "Streaming response chunk..."
                },
                Delta = "Streaming response chunk..."
            }
        };

        // Pre-serialize to get JSON strings
        _sessionJson = JsonSerializer.Serialize(_session, _sourceGenOptions);
        _messageJson = JsonSerializer.Serialize(_message, _sourceGenOptions);
        _messageWithPartsJson = JsonSerializer.Serialize(_messageWithParts, _sourceGenOptions);
        _eventJson = JsonSerializer.Serialize(_event, _sourceGenOptions);

        // Complex event with session data
        _complexEventJson = """
        {
            "type": "session.updated",
            "properties": {
                "session": {
                    "id": "ses_abc123def456",
                    "projectID": "proj_xyz789",
                    "directory": "/home/user/projects/myproject",
                    "title": "Test Session for Benchmarking",
                    "version": "1.0.0",
                    "time": {
                        "created": 1703123456789,
                        "updated": 1703123456789
                    },
                    "summary": {
                        "additions": 150,
                        "deletions": 45,
                        "files": 12
                    }
                }
            }
        }
        """;
    }

    #region Session Serialization

    [Benchmark(Description = "Session: Serialize (Source Gen)")]
    public string SerializeSession_SourceGen()
    {
        return JsonSerializer.Serialize(_session, _sourceGenOptions);
    }

    [Benchmark(Description = "Session: Deserialize (Source Gen)")]
    public Session? DeserializeSession_SourceGen()
    {
        return JsonSerializer.Deserialize<Session>(_sessionJson, _sourceGenOptions);
    }

    [Benchmark(Description = "Session: Serialize (Reflection)")]
    public string SerializeSession_Reflection()
    {
        return JsonSerializer.Serialize(_session, _reflectionOptions);
    }

    [Benchmark(Description = "Session: Deserialize (Reflection)")]
    public Session? DeserializeSession_Reflection()
    {
        return JsonSerializer.Deserialize<Session>(_sessionJson, _reflectionOptions);
    }

    #endregion

    #region Message Serialization

    [Benchmark(Description = "Message: Serialize (Source Gen)")]
    public string SerializeMessage_SourceGen()
    {
        return JsonSerializer.Serialize(_message, _sourceGenOptions);
    }

    [Benchmark(Description = "Message: Deserialize (Source Gen)")]
    public Message? DeserializeMessage_SourceGen()
    {
        return JsonSerializer.Deserialize<Message>(_messageJson, _sourceGenOptions);
    }

    #endregion

    #region MessageWithParts Serialization

    [Benchmark(Description = "MessageWithParts: Serialize (Source Gen)")]
    public string SerializeMessageWithParts_SourceGen()
    {
        return JsonSerializer.Serialize(_messageWithParts, _sourceGenOptions);
    }

    [Benchmark(Description = "MessageWithParts: Deserialize (Source Gen)")]
    public MessageWithParts? DeserializeMessageWithParts_SourceGen()
    {
        return JsonSerializer.Deserialize<MessageWithParts>(_messageWithPartsJson, _sourceGenOptions);
    }

    #endregion

    #region Event Serialization

    [Benchmark(Description = "Event: Serialize (Source Gen)")]
    public string SerializeEvent_SourceGen()
    {
        return JsonSerializer.Serialize(_event, _sourceGenOptions);
    }

    [Benchmark(Description = "Event: Deserialize (Source Gen)")]
    public Event? DeserializeEvent_SourceGen()
    {
        return JsonSerializer.Deserialize<Event>(_eventJson, _sourceGenOptions);
    }

    [Benchmark(Description = "ComplexEvent: Deserialize (Source Gen)")]
    public Event? DeserializeComplexEvent_SourceGen()
    {
        return JsonSerializer.Deserialize<Event>(_complexEventJson, _sourceGenOptions);
    }

    #endregion

    #region Roundtrip Benchmarks

    [Benchmark(Description = "Session: Roundtrip (Source Gen)")]
    public Session? SessionRoundtrip_SourceGen()
    {
        var json = JsonSerializer.Serialize(_session, _sourceGenOptions);
        return JsonSerializer.Deserialize<Session>(json, _sourceGenOptions);
    }

    [Benchmark(Description = "Message: Roundtrip (Source Gen)")]
    public Message? MessageRoundtrip_SourceGen()
    {
        var json = JsonSerializer.Serialize(_message, _sourceGenOptions);
        return JsonSerializer.Deserialize<Message>(json, _sourceGenOptions);
    }

    #endregion
}
