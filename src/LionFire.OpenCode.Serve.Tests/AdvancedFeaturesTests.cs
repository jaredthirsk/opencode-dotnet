using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

namespace LionFire.OpenCode.Serve.Tests;

/// <summary>
/// Tests for advanced API features including pagination, filtering, and progress callbacks.
/// </summary>
public class AdvancedFeaturesTests
{
    #region SessionListOptions Tests

    [Fact]
    public void SessionListOptions_DefaultValues_AreCorrect()
    {
        var options = new SessionListOptions();

        options.Limit.Should().BeNull();
        options.Skip.Should().BeNull();
        options.CreatedBefore.Should().BeNull();
        options.CreatedAfter.Should().BeNull();
        options.TitleSearch.Should().BeNull();
        options.Status.Should().BeNull();
        options.IsShared.Should().BeNull();
        options.HasParent.Should().BeNull();
        options.ParentId.Should().BeNull();
        options.SortOrder.Should().Be(SessionSortOrder.UpdatedDescending);
    }

    [Fact]
    public void SessionListOptions_CanSetLimit()
    {
        var options = new SessionListOptions { Limit = 10 };
        options.Limit.Should().Be(10);
    }

    [Fact]
    public void SessionListOptions_CanSetSkip()
    {
        var options = new SessionListOptions { Skip = 20 };
        options.Skip.Should().Be(20);
    }

    [Fact]
    public void SessionListOptions_CanSetDateFilters()
    {
        var testDate = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var options = new SessionListOptions
        {
            CreatedAfter = testDate,
            CreatedBefore = testDate.AddDays(7)
        };

        options.CreatedAfter.Should().Be(testDate);
        options.CreatedBefore.Should().Be(testDate.AddDays(7));
    }

    [Fact]
    public void SessionListOptions_CanSetTitleSearch()
    {
        var options = new SessionListOptions { TitleSearch = "test session" };
        options.TitleSearch.Should().Be("test session");
    }

    [Fact]
    public void SessionListOptions_CanSetStatus()
    {
        var options = new SessionListOptions { Status = "busy" };
        options.Status.Should().Be("busy");
    }

    [Fact]
    public void SessionListOptions_CanSetIsShared()
    {
        var options = new SessionListOptions { IsShared = true };
        options.IsShared.Should().BeTrue();
    }

    [Fact]
    public void SessionListOptions_CanSetSortOrder()
    {
        var options = new SessionListOptions { SortOrder = SessionSortOrder.CreatedAscending };
        options.SortOrder.Should().Be(SessionSortOrder.CreatedAscending);
    }

    [Fact]
    public void SessionListOptions_DefaultSortOrder_IsUpdatedDescending()
    {
        var options = new SessionListOptions();
        options.SortOrder.Should().Be(SessionSortOrder.UpdatedDescending);
    }

    [Fact]
    public void SessionListOptions_CanSetParentId()
    {
        var options = new SessionListOptions { ParentId = "ses_parent123" };
        options.ParentId.Should().Be("ses_parent123");
    }

    [Fact]
    public void SessionListOptions_CanSetHasParent()
    {
        var options = new SessionListOptions { HasParent = true };
        options.HasParent.Should().BeTrue();
    }

    #endregion

    #region MessageListOptions Tests

    [Fact]
    public void MessageListOptions_DefaultValues_AreCorrect()
    {
        var options = new MessageListOptions();

        options.Limit.Should().BeNull();
        options.Skip.Should().BeNull();
        options.Before.Should().BeNull();
        options.After.Should().BeNull();
        options.Role.Should().BeNull();
    }

    [Fact]
    public void MessageListOptions_CanSetLimitAndSkip()
    {
        var options = new MessageListOptions { Limit = 20, Skip = 40 };

        options.Limit.Should().Be(20);
        options.Skip.Should().Be(40);
    }

    [Fact]
    public void MessageListOptions_CanSetBefore()
    {
        var options = new MessageListOptions { Before = "msg_abc123" };
        options.Before.Should().Be("msg_abc123");
    }

    [Fact]
    public void MessageListOptions_CanSetAfter()
    {
        var options = new MessageListOptions { After = "msg_xyz789" };
        options.After.Should().Be("msg_xyz789");
    }

    [Fact]
    public void MessageListOptions_CanSetRole()
    {
        var options = new MessageListOptions { Role = "user" };
        options.Role.Should().Be("user");
    }

    [Fact]
    public void MessageListOptions_Validate_ThrowsWhenBothSkipAndBeforeSet()
    {
        var options = new MessageListOptions { Skip = 10, Before = "msg_abc" };

        Action act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*offset-based*cursor-based*");
    }

    [Fact]
    public void MessageListOptions_Validate_ThrowsWhenBothSkipAndAfterSet()
    {
        var options = new MessageListOptions { Skip = 10, After = "msg_abc" };

        Action act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*offset-based*cursor-based*");
    }

    [Fact]
    public void MessageListOptions_Validate_ThrowsWhenBothBeforeAndAfterSet()
    {
        var options = new MessageListOptions { Before = "msg_abc", After = "msg_xyz" };

        Action act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Before*After*");
    }

    [Fact]
    public void MessageListOptions_Validate_SucceedsForValidOffsetPagination()
    {
        var options = new MessageListOptions { Skip = 20, Limit = 10 };

        options.Validate().Should().BeTrue();
    }

    [Fact]
    public void MessageListOptions_Validate_SucceedsForValidCursorPagination()
    {
        var options = new MessageListOptions { Before = "msg_abc", Limit = 10 };

        options.Validate().Should().BeTrue();
    }

    #endregion

    #region StreamingProgress Tests

    [Fact]
    public void StreamingProgress_Starting_HasCorrectStatus()
    {
        var progress = StreamingProgress.Starting();

        progress.Status.Should().Be(StreamingStatus.Starting);
        progress.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void StreamingProgress_Connected_HasCorrectStatus()
    {
        var progress = StreamingProgress.Connected();

        progress.Status.Should().Be(StreamingStatus.Connected);
        progress.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void StreamingProgress_ChunkReceived_HasCorrectValues()
    {
        var elapsed = TimeSpan.FromSeconds(5);
        var progress = StreamingProgress.ChunkReceived(
            chunkCount: 10,
            bytesReceived: 5000,
            characterCount: 2500,
            elapsed: elapsed,
            latestChunk: "test chunk",
            eventType: "message");

        progress.Status.Should().Be(StreamingStatus.Receiving);
        progress.ChunkCount.Should().Be(10);
        progress.BytesReceived.Should().Be(5000);
        progress.CharacterCount.Should().Be(2500);
        progress.Elapsed.Should().Be(elapsed);
        progress.LatestChunk.Should().Be("test chunk");
        progress.EventType.Should().Be("message");
    }

    [Fact]
    public void StreamingProgress_Completed_HasCorrectValues()
    {
        var elapsed = TimeSpan.FromMinutes(2);
        var progress = StreamingProgress.Completed(
            chunkCount: 100,
            bytesReceived: 50000,
            characterCount: 25000,
            elapsed: elapsed);

        progress.Status.Should().Be(StreamingStatus.Completed);
        progress.ChunkCount.Should().Be(100);
        progress.BytesReceived.Should().Be(50000);
        progress.CharacterCount.Should().Be(25000);
        progress.Elapsed.Should().Be(elapsed);
        progress.Message.Should().Contain("completed");
    }

    [Fact]
    public void StreamingProgress_Failed_HasErrorInfo()
    {
        var exception = new InvalidOperationException("Test error");
        var elapsed = TimeSpan.FromSeconds(3);

        var progress = StreamingProgress.Failed(exception, chunkCount: 5, elapsed: elapsed);

        progress.Status.Should().Be(StreamingStatus.Error);
        progress.Error.Should().Be(exception);
        progress.ChunkCount.Should().Be(5);
        progress.Elapsed.Should().Be(elapsed);
        progress.Message.Should().Contain("Test error");
    }

    [Fact]
    public void StreamingProgress_Cancelled_HasCorrectStatus()
    {
        var elapsed = TimeSpan.FromSeconds(10);
        var progress = StreamingProgress.Cancelled(chunkCount: 15, elapsed: elapsed);

        progress.Status.Should().Be(StreamingStatus.Cancelled);
        progress.ChunkCount.Should().Be(15);
        progress.Elapsed.Should().Be(elapsed);
        progress.Message.Should().Contain("cancelled");
    }

    #endregion

    #region SessionSortOrder Tests

    [Fact]
    public void SessionSortOrder_AllValuesAreDefined()
    {
        // Verify all expected sort orders are defined
        var allValues = Enum.GetValues<SessionSortOrder>();

        allValues.Should().Contain(SessionSortOrder.CreatedDescending);
        allValues.Should().Contain(SessionSortOrder.CreatedAscending);
        allValues.Should().Contain(SessionSortOrder.UpdatedDescending);
        allValues.Should().Contain(SessionSortOrder.UpdatedAscending);
        allValues.Should().Contain(SessionSortOrder.TitleAscending);
        allValues.Should().Contain(SessionSortOrder.TitleDescending);
    }

    [Fact]
    public void SessionSortOrder_EnumValuesAreDistinct()
    {
        var allValues = Enum.GetValues<SessionSortOrder>();
        allValues.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region PaginatedResult Tests

    [Fact]
    public void PaginatedResult_Count_ReturnsItemsCount()
    {
        var result = new PaginatedResult<string>
        {
            Items = new List<string> { "a", "b", "c" }
        };

        result.Count.Should().Be(3);
    }

    [Fact]
    public void PaginatedResult_DefaultValues_AreCorrect()
    {
        var result = new PaginatedResult<int>();

        result.Items.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().BeNull();
        result.HasMore.Should().BeFalse();
        result.NextCursor.Should().BeNull();
        result.PreviousCursor.Should().BeNull();
    }

    #endregion
}
