using System.Buffers;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace LionFire.OpenCode.Serve.Internal;

/// <summary>
/// Object pool for StringBuilder instances to reduce allocations during SSE parsing.
/// </summary>
public static class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> Pool = new DefaultObjectPoolProvider()
        .CreateStringBuilderPool(initialCapacity: 256, maximumRetainedCapacity: 8192);

    /// <summary>
    /// Gets a StringBuilder from the pool.
    /// </summary>
    public static StringBuilder Get() => Pool.Get();

    /// <summary>
    /// Returns a StringBuilder to the pool after clearing it.
    /// </summary>
    public static void Return(StringBuilder builder)
    {
        builder.Clear();
        Pool.Return(builder);
    }

    /// <summary>
    /// Gets a StringBuilder, executes an action with it, and returns it to the pool.
    /// </summary>
    public static string GetString(Action<StringBuilder> action)
    {
        var sb = Get();
        try
        {
            action(sb);
            return sb.ToString();
        }
        finally
        {
            Return(sb);
        }
    }
}

/// <summary>
/// Helper class for working with ArrayPool to reduce heap allocations.
/// </summary>
public static class BufferPoolHelper
{
    private const int DefaultBufferSize = 4096;

    /// <summary>
    /// Rents a byte array from the shared pool.
    /// </summary>
    /// <param name="minimumSize">Minimum size needed.</param>
    /// <returns>A rented byte array (may be larger than requested).</returns>
    public static byte[] RentBytes(int minimumSize = DefaultBufferSize)
    {
        return ArrayPool<byte>.Shared.Rent(minimumSize);
    }

    /// <summary>
    /// Returns a byte array to the shared pool.
    /// </summary>
    /// <param name="buffer">The buffer to return.</param>
    /// <param name="clearArray">Whether to clear the array before returning.</param>
    public static void ReturnBytes(byte[] buffer, bool clearArray = false)
    {
        ArrayPool<byte>.Shared.Return(buffer, clearArray);
    }

    /// <summary>
    /// Rents a char array from the shared pool.
    /// </summary>
    /// <param name="minimumSize">Minimum size needed.</param>
    /// <returns>A rented char array (may be larger than requested).</returns>
    public static char[] RentChars(int minimumSize = DefaultBufferSize)
    {
        return ArrayPool<char>.Shared.Rent(minimumSize);
    }

    /// <summary>
    /// Returns a char array to the shared pool.
    /// </summary>
    /// <param name="buffer">The buffer to return.</param>
    /// <param name="clearArray">Whether to clear the array before returning.</param>
    public static void ReturnChars(char[] buffer, bool clearArray = false)
    {
        ArrayPool<char>.Shared.Return(buffer, clearArray);
    }
}

/// <summary>
/// Disposable wrapper for a rented array that auto-returns on dispose.
/// </summary>
/// <typeparam name="T">The element type of the array.</typeparam>
public readonly struct RentedArray<T> : IDisposable
{
    private readonly T[] _array;
    private readonly int _length;
    private readonly bool _clearOnReturn;

    /// <summary>
    /// Creates a new RentedArray by renting from the shared pool.
    /// </summary>
    /// <param name="minimumLength">Minimum length required.</param>
    /// <param name="clearOnReturn">Whether to clear array contents when returning.</param>
    public RentedArray(int minimumLength, bool clearOnReturn = false)
    {
        _array = ArrayPool<T>.Shared.Rent(minimumLength);
        _length = minimumLength;
        _clearOnReturn = clearOnReturn;
    }

    /// <summary>
    /// Gets the rented array.
    /// </summary>
    public T[] Array => _array;

    /// <summary>
    /// Gets the requested length (may be less than Array.Length).
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets a span over the usable portion of the array.
    /// </summary>
    public Span<T> Span => _array.AsSpan(0, _length);

    /// <summary>
    /// Gets a memory over the usable portion of the array.
    /// </summary>
    public Memory<T> Memory => _array.AsMemory(0, _length);

    /// <summary>
    /// Returns the array to the pool.
    /// </summary>
    public void Dispose()
    {
        if (_array != null)
        {
            ArrayPool<T>.Shared.Return(_array, _clearOnReturn);
        }
    }
}

/// <summary>
/// Span-based utilities for parsing SSE data lines.
/// </summary>
public static class SseParsingHelper
{
    private static readonly byte[] DataPrefixBytes = "data: "u8.ToArray();
    private static readonly byte[] NewLinePairBytes = "\n\n"u8.ToArray();

    /// <summary>
    /// Checks if a line starts with "data: " prefix using span comparison.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <returns>True if the line starts with "data: ".</returns>
    public static bool IsDataLine(ReadOnlySpan<char> line)
    {
        return line.StartsWith("data: ");
    }

    /// <summary>
    /// Checks if a byte span starts with "data: " prefix.
    /// </summary>
    /// <param name="line">The byte span to check.</param>
    /// <returns>True if the line starts with "data: ".</returns>
    public static bool IsDataLine(ReadOnlySpan<byte> line)
    {
        return line.StartsWith(DataPrefixBytes);
    }

    /// <summary>
    /// Extracts the JSON portion from an SSE data line.
    /// </summary>
    /// <param name="line">The full SSE data line.</param>
    /// <returns>The JSON content without the "data: " prefix.</returns>
    public static ReadOnlySpan<char> ExtractJsonFromDataLine(ReadOnlySpan<char> line)
    {
        if (!IsDataLine(line))
            return ReadOnlySpan<char>.Empty;

        return line.Slice(6).Trim();
    }

    /// <summary>
    /// Extracts the JSON portion from an SSE data line (byte version).
    /// </summary>
    /// <param name="line">The full SSE data line as bytes.</param>
    /// <returns>The JSON content without the "data: " prefix.</returns>
    public static ReadOnlySpan<byte> ExtractJsonFromDataLine(ReadOnlySpan<byte> line)
    {
        if (!IsDataLine(line))
            return ReadOnlySpan<byte>.Empty;

        return line.Slice(6).Trim((byte)' ');
    }

    /// <summary>
    /// Finds the next complete SSE event in a buffer.
    /// </summary>
    /// <param name="buffer">The buffer to search.</param>
    /// <param name="eventEnd">Output: the position after the event (including trailing newlines).</param>
    /// <returns>True if a complete event was found.</returns>
    public static bool TryFindNextEvent(ReadOnlySpan<byte> buffer, out int eventEnd)
    {
        var index = buffer.IndexOf(NewLinePairBytes);
        if (index >= 0)
        {
            eventEnd = index + 2; // Include the \n\n
            return true;
        }

        eventEnd = 0;
        return false;
    }
}

/// <summary>
/// Memory-efficient string operations.
/// </summary>
public static class StringUtilities
{
    /// <summary>
    /// Creates a string from a ReadOnlySpan without intermediate allocations.
    /// </summary>
    /// <param name="span">The span to convert.</param>
    /// <returns>A new string containing the span's contents.</returns>
    public static string CreateString(ReadOnlySpan<char> span)
    {
        return new string(span);
    }

    /// <summary>
    /// Checks if a string contains a substring using span comparison (case-insensitive).
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>True if the source contains the value.</returns>
    public static bool ContainsIgnoreCase(string source, string value)
    {
        return source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Efficiently concatenates strings using a rented buffer when beneficial.
    /// </summary>
    /// <param name="strings">The strings to concatenate.</param>
    /// <returns>The concatenated result.</returns>
    public static string ConcatEfficient(params string[] strings)
    {
        if (strings.Length == 0) return string.Empty;
        if (strings.Length == 1) return strings[0];

        // For small number of strings, use built-in concat
        if (strings.Length <= 4)
        {
            return string.Concat(strings);
        }

        // For larger arrays, use StringBuilder from pool
        return StringBuilderPool.GetString(sb =>
        {
            foreach (var s in strings)
            {
                sb.Append(s);
            }
        });
    }
}
