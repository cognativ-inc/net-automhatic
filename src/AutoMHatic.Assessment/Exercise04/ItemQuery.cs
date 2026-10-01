/*
 * Exercise 4 - In-memory item query
 * Difficulty: medium-hard  |  Suggested time: 10 minutes
 *
 * Implement ItemQuery.Execute: search, sort, and page a sequence of items.
 * Two identical requests must always return the same items in the same order,
 * so paging never skips or repeats an item.
 *
 * Search
 * - Keep items whose Name or ExternalId contains the search text, ignoring
 *   case. The result must be the same in every culture.
 * - Ignore surrounding whitespace in the search. A blank search keeps all items.
 *
 * Sorting
 * - Sort by CreatedAt, Name, ExternalId, or Status, in either direction. By
 *   default, the newest item comes first.
 * - Name and ExternalId sort ignoring case.
 * - When sorting by ExternalId, items without one always come last, in either
 *   direction.
 *
 * Paging
 * - Pages start at 1. PageSize must be from 1 to MaxPageSize.
 * - Throw ArgumentNullException for a null argument, and
 *   ArgumentOutOfRangeException when Page or PageSize is out of range.
 * - A page past the end is empty, but it still reports the totals.
 * - The source can be a sequence that can be read only once.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise04/ItemQueryTests.cs
 */

namespace AutoMHatic.Assessment.Exercise04;

public enum ItemStatus
{
    Draft,
    Active,
    Archived,
}

public enum ItemSortField
{
    CreatedAt,
    Name,
    ExternalId,
    Status,
}

public enum SortDirection
{
    Ascending,
    Descending,
}

public sealed record Item(Guid Id, string Name, string? ExternalId, ItemStatus Status, DateTimeOffset CreatedAt);

public sealed record ItemQueryOptions
{
    public string? Search { get; init; }

    public ItemSortField SortBy { get; init; } = ItemSortField.CreatedAt;

    public SortDirection Direction { get; init; } = SortDirection.Descending;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

public sealed record ItemPage(IReadOnlyList<Item> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public static class ItemQuery
{
    public const int MaxPageSize = 100;

    public static ItemPage Execute(IEnumerable<Item> items, ItemQueryOptions options)
    {
        throw new NotImplementedException();
    }
}
