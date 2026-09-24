using System.Collections;
using System.Globalization;
using AutoMHatic.Assessment.Exercise04;

namespace AutoMHatic.Assessment.Tests.Exercise04;

public sealed class ApplicationQueueTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void C01_Invalid_arguments_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => ApplicationQueue.Query(null!, new QueueQuery()));
        Assert.Throws<ArgumentNullException>(() => ApplicationQueue.Query([], null!));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, 101)]
    public void C02_Page_and_page_size_must_be_in_range(int page, int pageSize)
    {
        var query = new QueueQuery { Page = page, PageSize = pageSize };

        Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationQueue.Query([], query));
    }

    [Fact]
    public void C03_An_empty_queue_returns_an_empty_page()
    {
        var page = ApplicationQueue.Query([], new QueueQuery());

        Assert.Empty(page.Items);
        Assert.Equal(new QueuePage(page.Items, 0, 1, 20, 0, 0, 0), page);
    }

    [Fact]
    public void C04_By_default_the_newest_application_comes_first()
    {
        var old = Item("Old", receivedMinutesAgo: 90);
        var newest = Item("Newest", receivedMinutesAgo: 1);
        var middle = Item("Middle", receivedMinutesAgo: 30);

        var page = ApplicationQueue.Query([old, newest, middle], new QueueQuery());

        Assert.Equal([newest, middle, old], page.Items);
    }

    [Fact]
    public void C05_Search_matches_part_of_the_name_or_loan_id_ignoring_case()
    {
        var smith = Item("Smith - Manufactured home", loanId: null);
        var jones = Item("Jones refinance", loanId: "E000036114");
        var other = Item("Garcia purchase", loanId: "E000099999");

        Assert.Equal([smith], Search([smith, jones, other], "SMITH"));
        Assert.Equal([jones], Search([smith, jones, other], "36114"));
        Assert.Equal([jones], Search([smith, jones, other], "e0000361"));
        Assert.Empty(Search([smith, jones, other], "nobody"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void C06_A_blank_search_does_not_filter(string? search)
    {
        QueueItem[] items = [Item("A"), Item("B"), Item("C")];

        Assert.Equal(3, Search(items, search).Count);
    }

    [Fact]
    public void C07_Search_ignores_surrounding_whitespace()
    {
        var jones = Item("Jones refinance", loanId: "E000036114");

        Assert.Equal([jones], Search([jones, Item("Other")], "  jones "));
    }

    [Fact]
    public void C08_Search_behaves_the_same_in_every_locale()
    {
        var ives = Item("LINDA IVES", loanId: null);
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Assert.Equal([ives], Search([ives, Item("Other")], "ives"));
            Assert.Equal([ives], Search([ives, Item("Other")], "Linda"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void C09_Sorting_by_name_ignores_case()
    {
        var alpha = Item("alpha");
        var bravo = Item("Bravo");
        var charlie = Item("charlie");

        var ascending = Sort([charlie, alpha, bravo], QueueSortField.ApplicationName, SortDirection.Ascending);
        var descending = Sort([charlie, alpha, bravo], QueueSortField.ApplicationName, SortDirection.Descending);

        Assert.Equal([alpha, bravo, charlie], ascending);
        Assert.Equal([charlie, bravo, alpha], descending);
    }

    [Fact]
    public void C10_Sorting_by_status_and_received_in_both_directions()
    {
        var pending = Item("P", status: QueueStatus.Pending, receivedMinutesAgo: 10);
        var completed = Item("C", status: QueueStatus.Completed, receivedMinutesAgo: 20);

        Assert.Equal([pending, completed], Sort([completed, pending], QueueSortField.Status, SortDirection.Ascending));
        Assert.Equal([completed, pending], Sort([pending, completed], QueueSortField.Status, SortDirection.Descending));
        Assert.Equal([completed, pending], Sort([pending, completed], QueueSortField.Received, SortDirection.Ascending));
    }

    [Fact]
    public void C11_Applications_without_a_loan_id_stay_at_the_bottom()
    {
        var first = Item("First", loanId: "E000000001");
        var second = Item("Second", loanId: "E000000002");
        var missing = Item("Missing", loanId: null);

        var ascending = Sort([missing, second, first], QueueSortField.LoanId, SortDirection.Ascending);
        var descending = Sort([missing, first, second], QueueSortField.LoanId, SortDirection.Descending);

        Assert.Equal([first, second, missing], ascending);
        Assert.Equal([second, first, missing], descending);
    }

    [Fact]
    public void C12_Pages_split_the_results_and_describe_the_range()
    {
        var items = Enumerable.Range(0, 45).Select(i => Item($"App {i:D2}", receivedMinutesAgo: i)).ToList();

        var page = ApplicationQueue.Query(items, new QueueQuery { Page = 2, PageSize = 20 });
        var last = ApplicationQueue.Query(items, new QueueQuery { Page = 3, PageSize = 20 });

        Assert.Equal(items.Skip(20).Take(20), page.Items);
        Assert.Equal(new QueuePage(page.Items, 45, 2, 20, 3, 21, 40), page);
        Assert.Equal(items.Skip(40), last.Items);
        Assert.Equal(new QueuePage(last.Items, 45, 3, 20, 3, 41, 45), last);
    }

    [Fact]
    public void C13_Totals_reflect_the_search_not_the_whole_queue()
    {
        var items = Enumerable.Range(0, 30)
            .Select(i => Item(i % 3 == 0 ? $"Smith {i}" : $"Jones {i}", receivedMinutesAgo: i))
            .ToList();

        var page = ApplicationQueue.Query(items, new QueueQuery { Search = "smith", PageSize = 4 });

        Assert.Equal(10, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(4, page.Items.Count);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(int.MaxValue)]
    public void C14_A_page_past_the_end_is_empty_but_keeps_the_totals(int pageNumber)
    {
        var items = Enumerable.Range(0, 5).Select(i => Item($"App {i}")).ToList();

        var page = ApplicationQueue.Query(items, new QueueQuery { Page = pageNumber, PageSize = 2 });

        Assert.Empty(page.Items);
        Assert.Equal(new QueuePage(page.Items, 5, pageNumber, 2, 3, 0, 0), page);
    }

    [Fact]
    public void C15_Equal_sort_keys_produce_the_same_order_every_time()
    {
        var items = Enumerable.Range(0, 12).Select(i => Item("Same name", receivedMinutesAgo: 5)).ToList();
        var reversed = Enumerable.Reverse(items).ToList();
        var query = new QueueQuery { SortBy = QueueSortField.ApplicationName, PageSize = 5 };

        for (var pageNumber = 1; pageNumber <= 3; pageNumber++)
        {
            var fromOriginal = ApplicationQueue.Query(items, query with { Page = pageNumber });
            var fromReversed = ApplicationQueue.Query(reversed, query with { Page = pageNumber });

            Assert.Equal(fromOriginal.Items, fromReversed.Items);
        }
    }

    [Fact]
    public void C16_The_source_is_enumerated_only_once()
    {
        var source = new SinglePassEnumerable([Item("Smith"), Item("Jones"), Item("Smithers")]);

        var page = ApplicationQueue.Query(source, new QueueQuery { Search = "smith", PageSize = 1 });

        Assert.Equal(2, page.TotalCount);
        Assert.Single(page.Items);
    }

    private static List<QueueItem> Search(IEnumerable<QueueItem> items, string? search) =>
        [.. ApplicationQueue.Query(items, new QueueQuery { Search = search }).Items];

    private static List<QueueItem> Sort(IEnumerable<QueueItem> items, QueueSortField field, SortDirection direction) =>
        [.. ApplicationQueue.Query(items, new QueueQuery { SortBy = field, Direction = direction }).Items];

    private static QueueItem Item(
        string name,
        string? loanId = null,
        QueueStatus status = QueueStatus.Pending,
        int receivedMinutesAgo = 0) =>
        new(Guid.NewGuid(), name, loanId, status, Now.AddMinutes(-receivedMinutesAgo));

    private sealed class SinglePassEnumerable(IEnumerable<QueueItem> items) : IEnumerable<QueueItem>
    {
        private bool _enumerated;

        public IEnumerator<QueueItem> GetEnumerator()
        {
            Assert.False(_enumerated, "The source sequence was enumerated more than once.");
            _enumerated = true;
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
