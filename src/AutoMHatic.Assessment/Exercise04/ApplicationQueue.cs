/*
 * Exercise 4 - Application queue
 * Difficulty: medium-hard  |  Suggested time: 10 minutes
 *
 * Premise
 * -------
 * The Application Queue is the first screen a Data Entry operator sees after
 * sign-in. It lists the loan applications waiting for review, and operators use
 * it all day to decide what to open next. The table must support:
 *
 * - A single search box that matches part of the Application Name or part of the
 *   Loan ID (for example "E000036114"), ignoring case.
 * - Sorting by Received, Application Name, Loan ID, or Status. By default the
 *   newest application appears first.
 * - Pagination with a page-size selector and a caption such as
 *   "Showing 21 to 40 of 263". Pages start at 1. The page size can be at most
 *   MaxPageSize.
 *
 * Applications receive a Loan ID only after they reach Encompass, so many rows
 * do not have one yet. Operators want those rows at the bottom when they sort
 * by Loan ID, in either direction.
 *
 * The operators work in many locales, and the queue must behave the same for
 * all of them. Two identical requests must always return the same rows in the
 * same order, so that paging never skips or repeats a row.
 *
 * Your task
 * ---------
 * Implement ApplicationQueue.Query. The request and result types are given.
 * You may add members and helper types.
 *
 * - Throw ArgumentNullException when items or query is null.
 * - Throw ArgumentOutOfRangeException when Page is below 1, or PageSize is below
 *   1 or above MaxPageSize.
 * - A page past the end is empty, but it still reports the totals.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise04/ApplicationQueueTests.cs
 */

namespace AutoMHatic.Assessment.Exercise04;

public enum QueueStatus
{
    Pending,
    Completed,
}

public enum QueueSortField
{
    Received,
    ApplicationName,
    LoanId,
    Status,
}

public enum SortDirection
{
    Ascending,
    Descending,
}

public sealed record QueueItem(Guid Id, string ApplicationName, string? LoanId, QueueStatus Status, DateTimeOffset ReceivedAt);

public sealed record QueueQuery
{
    public string? Search { get; init; }

    public QueueSortField SortBy { get; init; } = QueueSortField.Received;

    public SortDirection Direction { get; init; } = SortDirection.Descending;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

public sealed record QueuePage(
    IReadOnlyList<QueueItem> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    int ShowingFrom,
    int ShowingTo);

public static class ApplicationQueue
{
    public const int MaxPageSize = 100;

    public static QueuePage Query(IEnumerable<QueueItem> items, QueueQuery query)
    {
        throw new NotImplementedException();
    }
}
