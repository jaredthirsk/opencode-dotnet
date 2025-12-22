namespace LionFire.OpenCode.Serve.Models;

/// <summary>
/// Options for filtering and paginating session lists.
/// </summary>
/// <remarks>
/// All filter properties are optional. When not specified, no filtering is applied
/// for that property. Multiple filters are combined with AND logic.
/// </remarks>
public sealed class SessionListOptions
{
    /// <summary>
    /// Gets or sets the maximum number of sessions to return.
    /// </summary>
    /// <remarks>
    /// When null, the server default limit is used.
    /// </remarks>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the number of sessions to skip (for offset-based pagination).
    /// </summary>
    /// <remarks>
    /// Combined with <see cref="Limit"/>, this enables page-based navigation.
    /// For example, to get page 3 with 10 items per page: Skip = 20, Limit = 10.
    /// </remarks>
    public int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the filter for sessions created before this date/time.
    /// </summary>
    /// <remarks>
    /// The time is specified in UTC. Sessions created exactly at this time
    /// are not included in the results.
    /// </remarks>
    public DateTimeOffset? CreatedBefore { get; set; }

    /// <summary>
    /// Gets or sets the filter for sessions created after this date/time.
    /// </summary>
    /// <remarks>
    /// The time is specified in UTC. Sessions created exactly at this time
    /// are not included in the results.
    /// </remarks>
    public DateTimeOffset? CreatedAfter { get; set; }

    /// <summary>
    /// Gets or sets the filter for sessions updated before this date/time.
    /// </summary>
    public DateTimeOffset? UpdatedBefore { get; set; }

    /// <summary>
    /// Gets or sets the filter for sessions updated after this date/time.
    /// </summary>
    public DateTimeOffset? UpdatedAfter { get; set; }

    /// <summary>
    /// Gets or sets the search text to filter sessions by title.
    /// </summary>
    /// <remarks>
    /// When specified, only sessions whose title contains this text
    /// (case-insensitive) are returned.
    /// </remarks>
    public string? TitleSearch { get; set; }

    /// <summary>
    /// Gets or sets the session status filter.
    /// </summary>
    /// <remarks>
    /// When specified, only sessions in the given status are returned.
    /// Valid values include "idle", "busy", "retry".
    /// </remarks>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets whether to include only shared sessions.
    /// </summary>
    /// <remarks>
    /// When true, only sessions that have been shared are returned.
    /// When false, only non-shared sessions are returned.
    /// When null (default), no filter is applied.
    /// </remarks>
    public bool? IsShared { get; set; }

    /// <summary>
    /// Gets or sets whether to include only sessions with a parent (forked sessions).
    /// </summary>
    /// <remarks>
    /// When true, only forked sessions (those with a parent) are returned.
    /// When false, only root sessions (no parent) are returned.
    /// When null (default), no filter is applied.
    /// </remarks>
    public bool? HasParent { get; set; }

    /// <summary>
    /// Gets or sets the parent session ID to filter by.
    /// </summary>
    /// <remarks>
    /// When specified, only direct children of this session are returned.
    /// </remarks>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the sort order for results.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SessionSortOrder.UpdatedDescending"/> (most recently updated first).
    /// </remarks>
    public SessionSortOrder SortOrder { get; set; } = SessionSortOrder.UpdatedDescending;

    /// <summary>
    /// Converts the options to query parameters for the API request.
    /// </summary>
    internal Dictionary<string, string> ToQueryParameters()
    {
        var parameters = new Dictionary<string, string>();

        if (Limit.HasValue)
            parameters["limit"] = Limit.Value.ToString();

        if (Skip.HasValue)
            parameters["skip"] = Skip.Value.ToString();

        if (CreatedBefore.HasValue)
            parameters["createdBefore"] = CreatedBefore.Value.ToUnixTimeMilliseconds().ToString();

        if (CreatedAfter.HasValue)
            parameters["createdAfter"] = CreatedAfter.Value.ToUnixTimeMilliseconds().ToString();

        if (UpdatedBefore.HasValue)
            parameters["updatedBefore"] = UpdatedBefore.Value.ToUnixTimeMilliseconds().ToString();

        if (UpdatedAfter.HasValue)
            parameters["updatedAfter"] = UpdatedAfter.Value.ToUnixTimeMilliseconds().ToString();

        if (!string.IsNullOrEmpty(TitleSearch))
            parameters["titleSearch"] = TitleSearch;

        if (!string.IsNullOrEmpty(Status))
            parameters["status"] = Status;

        if (IsShared.HasValue)
            parameters["isShared"] = IsShared.Value.ToString().ToLowerInvariant();

        if (HasParent.HasValue)
            parameters["hasParent"] = HasParent.Value.ToString().ToLowerInvariant();

        if (!string.IsNullOrEmpty(ParentId))
            parameters["parentID"] = ParentId;

        if (SortOrder != SessionSortOrder.UpdatedDescending)
            parameters["sort"] = SortOrder.ToApiValue();

        return parameters;
    }
}

/// <summary>
/// Sort order for session list results.
/// </summary>
public enum SessionSortOrder
{
    /// <summary>
    /// Sort by creation time, most recent first (default).
    /// </summary>
    CreatedDescending,

    /// <summary>
    /// Sort by creation time, oldest first.
    /// </summary>
    CreatedAscending,

    /// <summary>
    /// Sort by update time, most recently updated first.
    /// </summary>
    UpdatedDescending,

    /// <summary>
    /// Sort by update time, least recently updated first.
    /// </summary>
    UpdatedAscending,

    /// <summary>
    /// Sort by title alphabetically (A-Z).
    /// </summary>
    TitleAscending,

    /// <summary>
    /// Sort by title reverse alphabetically (Z-A).
    /// </summary>
    TitleDescending
}

/// <summary>
/// Extension methods for <see cref="SessionSortOrder"/>.
/// </summary>
internal static class SessionSortOrderExtensions
{
    /// <summary>
    /// Converts the sort order to the API query parameter value.
    /// </summary>
    public static string ToApiValue(this SessionSortOrder sortOrder) => sortOrder switch
    {
        SessionSortOrder.CreatedDescending => "created_desc",
        SessionSortOrder.CreatedAscending => "created_asc",
        SessionSortOrder.UpdatedDescending => "updated_desc",
        SessionSortOrder.UpdatedAscending => "updated_asc",
        SessionSortOrder.TitleAscending => "title_asc",
        SessionSortOrder.TitleDescending => "title_desc",
        _ => "updated_desc"
    };
}

/// <summary>
/// Options for filtering and paginating message lists.
/// </summary>
/// <remarks>
/// <para>
/// This class supports two pagination strategies:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>Offset-based</strong>: Use <see cref="Skip"/> and <see cref="Limit"/> for page-based navigation.
/// Simple but may have consistency issues with concurrent modifications.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Cursor-based</strong>: Use <see cref="Before"/> or <see cref="After"/> for stable pagination.
/// Recommended for large lists or real-time scenarios.
/// </description>
/// </item>
/// </list>
/// </remarks>
public sealed class MessageListOptions
{
    /// <summary>
    /// Gets or sets the maximum number of messages to return.
    /// </summary>
    /// <remarks>
    /// When null, the server default limit is used. Maximum recommended value is 100.
    /// </remarks>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the number of messages to skip (for offset-based pagination).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This enables traditional page-based navigation but may have consistency issues
    /// if messages are added or removed during pagination.
    /// </para>
    /// <para>
    /// For more consistent pagination in real-time scenarios, use cursor-based
    /// pagination with <see cref="Before"/> or <see cref="After"/> instead.
    /// </para>
    /// </remarks>
    public int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the message ID cursor for fetching messages before this message.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When specified, only messages that appear before this message in the conversation
    /// are returned. This is useful for loading older message history.
    /// </para>
    /// <para>
    /// This is cursor-based pagination and provides more consistent results than
    /// offset-based pagination when messages may be added during the request.
    /// </para>
    /// </remarks>
    public string? Before { get; set; }

    /// <summary>
    /// Gets or sets the message ID cursor for fetching messages after this message.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When specified, only messages that appear after this message in the conversation
    /// are returned. This is useful for loading newer messages.
    /// </para>
    /// <para>
    /// This is cursor-based pagination and provides more consistent results than
    /// offset-based pagination when messages may be added during the request.
    /// </para>
    /// </remarks>
    public string? After { get; set; }

    /// <summary>
    /// Gets or sets the filter for messages by role.
    /// </summary>
    /// <remarks>
    /// Valid values are "user" or "assistant". When null, messages of all roles are returned.
    /// </remarks>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets whether to include only messages from a specific time range (created before).
    /// </summary>
    public DateTimeOffset? CreatedBefore { get; set; }

    /// <summary>
    /// Gets or sets whether to include only messages from a specific time range (created after).
    /// </summary>
    public DateTimeOffset? CreatedAfter { get; set; }

    /// <summary>
    /// Validates that the options are consistent.
    /// </summary>
    /// <returns>True if the options are valid; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when conflicting options are set.</exception>
    public bool Validate()
    {
        // Check for conflicting pagination options
        if ((Skip.HasValue || Skip > 0) && (!string.IsNullOrEmpty(Before) || !string.IsNullOrEmpty(After)))
        {
            throw new InvalidOperationException(
                "Cannot use offset-based pagination (Skip) with cursor-based pagination (Before/After). " +
                "Choose one pagination strategy.");
        }

        if (!string.IsNullOrEmpty(Before) && !string.IsNullOrEmpty(After))
        {
            throw new InvalidOperationException(
                "Cannot specify both Before and After cursors. Use one or the other.");
        }

        return true;
    }

    /// <summary>
    /// Converts the options to query parameters for the API request.
    /// </summary>
    internal Dictionary<string, string> ToQueryParameters()
    {
        var parameters = new Dictionary<string, string>();

        if (Limit.HasValue)
            parameters["limit"] = Limit.Value.ToString();

        if (Skip.HasValue)
            parameters["skip"] = Skip.Value.ToString();

        if (!string.IsNullOrEmpty(Before))
            parameters["before"] = Before;

        if (!string.IsNullOrEmpty(After))
            parameters["after"] = After;

        if (!string.IsNullOrEmpty(Role))
            parameters["role"] = Role;

        if (CreatedBefore.HasValue)
            parameters["createdBefore"] = CreatedBefore.Value.ToUnixTimeMilliseconds().ToString();

        if (CreatedAfter.HasValue)
            parameters["createdAfter"] = CreatedAfter.Value.ToUnixTimeMilliseconds().ToString();

        return parameters;
    }
}

/// <summary>
/// Result of a paginated list operation with metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class PaginatedResult<T>
{
    /// <summary>
    /// Gets or sets the items in the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the total count of items (if available from the server).
    /// </summary>
    /// <remarks>
    /// This may be null if the server does not provide a total count.
    /// </remarks>
    public int? TotalCount { get; set; }

    /// <summary>
    /// Gets or sets whether there are more items available.
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    /// Gets or sets the cursor for fetching the next page (if using cursor-based pagination).
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// Gets or sets the cursor for fetching the previous page (if using cursor-based pagination).
    /// </summary>
    public string? PreviousCursor { get; set; }

    /// <summary>
    /// Gets the count of items in the current page.
    /// </summary>
    public int Count => Items.Count;
}
