using System.Buffers;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using LionFire.OpenCode.Serve.Internal;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.Models.Events;

namespace LionFire.OpenCode.Serve.Benchmarks;

/// <summary>
/// Benchmarks for SSE (Server-Sent Events) streaming event parsing.
/// Target: &lt;5ms overhead for streaming operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class StreamingEventBenchmarks
{
    private string _sseEventSmall = null!;
    private string _sseEventMedium = null!;
    private string _sseEventLarge = null!;
    private string[] _sseEventStream = null!;
    private byte[] _sseEventStreamBytes = null!;

    private JsonSerializerOptions _options = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = JsonOptions.SourceGenerated;

        // Small SSE event - text delta
        _sseEventSmall = """
data: {"type":"message.part.updated","properties":{"sessionID":"ses_123","messageID":"msg_456","delta":"Hello"}}

""";

        // Medium SSE event - text part with full data
        _sseEventMedium = """
data: {"type":"message.part.updated","properties":{"sessionID":"ses_abc123def456","messageID":"msg_test123456","part":{"id":"prt_001","sessionID":"ses_abc123def456","messageID":"msg_test123456","type":"text","text":"This is a medium-sized response that contains more detailed information about the topic at hand."},"delta":"This is a medium-sized response that contains more detailed information about the topic at hand."}}

""";

        // Large SSE event - tool call with input/output
        _sseEventLarge = """
data: {"type":"message.part.updated","properties":{"sessionID":"ses_abc123def456","messageID":"msg_test123456","part":{"id":"prt_001","sessionID":"ses_abc123def456","messageID":"msg_test123456","type":"tool","tool":"Bash","callID":"call_xyz123","state":"completed","input":{"command":"find /src -name '*.cs' -type f | head -20"},"output":"Found 20 files:\n/src/file1.cs\n/src/file2.cs\n/src/file3.cs\n/src/file4.cs\n/src/file5.cs\n/src/file6.cs\n/src/file7.cs\n/src/file8.cs\n/src/file9.cs\n/src/file10.cs\n/src/file11.cs\n/src/file12.cs\n/src/file13.cs\n/src/file14.cs\n/src/file15.cs\n/src/file16.cs\n/src/file17.cs\n/src/file18.cs\n/src/file19.cs\n/src/file20.cs"}}}

""";

        // Simulate a stream of SSE events (like during a real response)
        _sseEventStream = GenerateSseEventStream(50);
        _sseEventStreamBytes = Encoding.UTF8.GetBytes(string.Join("", _sseEventStream));
    }

    private static string[] GenerateSseEventStream(int count)
    {
        var events = new string[count];
        var baseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                // Session created event
                events[i] = $"data: {{\"type\":\"session.created\",\"properties\":{{\"session\":{{\"id\":\"ses_bench123\",\"projectID\":\"proj_xyz\",\"directory\":\"/home/user/project\",\"title\":\"Benchmark Session\",\"version\":\"1.0.0\",\"time\":{{\"created\":{baseTime},\"updated\":{baseTime}}}}}}}}}\n\n";
            }
            else if (i == count - 1)
            {
                // Session idle event
                events[i] = "data: {\"type\":\"session.idle\",\"properties\":{\"sessionID\":\"ses_bench123\"}}\n\n";
            }
            else if (i % 10 == 0)
            {
                // Occasional tool call
                events[i] = $"data: {{\"type\":\"message.part.updated\",\"properties\":{{\"sessionID\":\"ses_bench123\",\"messageID\":\"msg_{i:D4}\",\"part\":{{\"id\":\"prt_{i:D4}\",\"sessionID\":\"ses_bench123\",\"messageID\":\"msg_{i:D4}\",\"type\":\"tool\",\"tool\":\"Read\",\"callID\":\"call_{i:D4}\",\"state\":\"completed\"}}}}}}\n\n";
            }
            else
            {
                // Text delta
                events[i] = $"data: {{\"type\":\"message.part.updated\",\"properties\":{{\"sessionID\":\"ses_bench123\",\"messageID\":\"msg_0001\",\"delta\":\"Chunk {i}: This is streaming text content. \"}}}}\n\n";
            }
        }

        return events;
    }

    #region Single Event Parsing

    [Benchmark(Description = "Parse: Small SSE event")]
    public Event? ParseSmallSseEvent()
    {
        var jsonLine = _sseEventSmall.Substring(6, _sseEventSmall.Length - 8); // Remove "data: " and trailing newlines
        return JsonSerializer.Deserialize<Event>(jsonLine, _options);
    }

    [Benchmark(Description = "Parse: Medium SSE event")]
    public Event? ParseMediumSseEvent()
    {
        var jsonLine = _sseEventMedium.Substring(6, _sseEventMedium.Length - 8);
        return JsonSerializer.Deserialize<Event>(jsonLine, _options);
    }

    [Benchmark(Description = "Parse: Large SSE event")]
    public Event? ParseLargeSseEvent()
    {
        var jsonLine = _sseEventLarge.Substring(6, _sseEventLarge.Length - 8);
        return JsonSerializer.Deserialize<Event>(jsonLine, _options);
    }

    #endregion

    #region Stream Parsing

    [Benchmark(Description = "Parse: 50 SSE events (string array)")]
    public List<Event?> ParseSseEventStream()
    {
        var events = new List<Event?>(_sseEventStream.Length);

        foreach (var eventLine in _sseEventStream)
        {
            if (eventLine.StartsWith("data: "))
            {
                var jsonLine = eventLine.Substring(6, eventLine.Length - 8);
                var ev = JsonSerializer.Deserialize<Event>(jsonLine, _options);
                events.Add(ev);
            }
        }

        return events;
    }

    [Benchmark(Description = "Parse: 50 SSE events (ReadOnlySpan)")]
    public List<Event?> ParseSseEventStreamSpan()
    {
        var events = new List<Event?>(_sseEventStream.Length);
        ReadOnlySpan<char> dataPrefix = "data: ";

        foreach (var eventLine in _sseEventStream)
        {
            var span = eventLine.AsSpan();
            if (span.StartsWith(dataPrefix))
            {
                var jsonSpan = span.Slice(6, span.Length - 8);
                var ev = JsonSerializer.Deserialize<Event>(jsonSpan, _options);
                events.Add(ev);
            }
        }

        return events;
    }

    [Benchmark(Description = "Parse: 50 SSE events (UTF8 bytes)")]
    public List<Event?> ParseSseEventStreamBytes()
    {
        var events = new List<Event?>(50);
        var span = _sseEventStreamBytes.AsSpan();
        var dataPrefix = "data: "u8;
        var newline = "\n\n"u8;

        int position = 0;
        while (position < span.Length)
        {
            // Find next newline pair
            var remaining = span.Slice(position);
            var newlineIndex = remaining.IndexOf(newline);
            if (newlineIndex < 0) break;

            var line = remaining.Slice(0, newlineIndex);
            position += newlineIndex + 2;

            if (line.StartsWith(dataPrefix))
            {
                var jsonBytes = line.Slice(6);
                var ev = JsonSerializer.Deserialize<Event>(jsonBytes, _options);
                events.Add(ev);
            }
        }

        return events;
    }

    #endregion

    #region StringBuilder Simulation (Current Implementation)

    [Benchmark(Description = "StringBuilder: Accumulate + Parse")]
    public Event? StringBuilderAccumulateAndParse()
    {
        var sb = new StringBuilder();

        // Simulate accumulating chunks like StreamReader does
        foreach (var chunk in SplitIntoChunks(_sseEventMedium, 20))
        {
            sb.Append(chunk);
        }

        var fullLine = sb.ToString();
        if (fullLine.StartsWith("data: "))
        {
            var json = fullLine.Substring(6).TrimEnd();
            return JsonSerializer.Deserialize<Event>(json, _options);
        }

        return null;
    }

    private static IEnumerable<string> SplitIntoChunks(string str, int chunkSize)
    {
        for (int i = 0; i < str.Length; i += chunkSize)
        {
            yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
        }
    }

    #endregion

    #region Event Type Handling

    [Benchmark(Description = "Type switch: Handle event types")]
    public string HandleEventType()
    {
        var jsonLine = _sseEventMedium.Substring(6, _sseEventMedium.Length - 8);
        var ev = JsonSerializer.Deserialize<Event>(jsonLine, _options);

        return ev switch
        {
            MessagePartUpdatedEvent mpu => $"Part updated: {mpu.Properties?.Part?.Type}",
            SessionCreatedEvent sc => $"Session: {sc.Properties?.Session?.Id}",
            SessionIdleEvent si => $"Idle: {si.Properties?.SessionId}",
            _ => "Unknown event"
        };
    }

    #endregion

    #region Delta Extraction

    [Benchmark(Description = "Extract: Delta from event")]
    public string? ExtractDelta()
    {
        var jsonLine = _sseEventMedium.Substring(6, _sseEventMedium.Length - 8);
        var ev = JsonSerializer.Deserialize<Event>(jsonLine, _options);

        if (ev is MessagePartUpdatedEvent mpu)
        {
            return mpu.Properties?.Delta;
        }

        return null;
    }

    [Benchmark(Description = "Extract: Delta using JSON DOM")]
    public string? ExtractDeltaJsonDocument()
    {
        var jsonLine = _sseEventMedium.Substring(6, _sseEventMedium.Length - 8);
        using var doc = JsonDocument.Parse(jsonLine);

        if (doc.RootElement.TryGetProperty("properties", out var props) &&
            props.TryGetProperty("delta", out var delta))
        {
            return delta.GetString();
        }

        return null;
    }

    #endregion
}
