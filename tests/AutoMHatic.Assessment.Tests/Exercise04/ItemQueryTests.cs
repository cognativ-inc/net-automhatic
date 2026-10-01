using System.Collections;
using System.Globalization;
using AutoMHatic.Assessment.Exercise04;

namespace AutoMHatic.Assessment.Tests.Exercise04;

public sealed class ItemQueryTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void C01_Null_arguments_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => ItemQuery.Execute(null!, new ItemQueryOptions()));
        Assert.Throws<ArgumentNullException>(() => ItemQuery.Execute([], null!));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, 101)]
    public void C02_Page_and_page_size_must_be_in_range(int page, int pageSize)
    {
        var options = new ItemQueryOptions { Page = page, PageSize = pageSize };

        Assert.Throws<ArgumentOutOfRangeException>(() => ItemQuery.Execute([], options));
    }

    [Fact]
    public void C03_An_empty_source_returns_an_empty_first_page()
    {
        var page = ItemQuery.Execute([], new ItemQueryOptions());

        Assert.Empty(page.Items);
        Assert.Equal(new ItemPage(page.Items, TotalCount: 0, Page: 1, PageSize: 20, TotalPages: 0), page);
    }

    [Fact]
    public void C04_By_default_the_newest_item_comes_first()
    {
        var old = NewItem("Old", createdMinutesAgo: 90);
        var newest = NewItem("Newest", createdMinutesAgo: 1);
        var middle = NewItem("Middle", createdMinutesAgo: 30);

        var page = ItemQuery.Execute([old, newest, middle], new ItemQueryOptions());

        Assert.Equal([newest, middle, old], page.Items);
    }

    [Fact]
    public void C05_Search_matches_part_of_the_name_or_external_id_ignoring_case()
    {
        var report = NewItem("Quarterly report", externalId: null);
        var invoice = NewItem("Invoice batch", externalId: "INV-2026-0042");
        var other = NewItem("Archive export", externalId: "EXP-0007");

        Item[] items = [report, invoice, other];

        Assert.Equal([report], Search(items, "REPORT"));
        Assert.Equal([invoice], Search(items, "0042"));
        Assert.Equal([invoice], Search(items, "inv-2026"));
        Assert.Empty(Search(items, "missing"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void C06_A_blank_search_keeps_every_item(string? search)
    {
        Item[] items = [NewItem("A"), NewItem("B"), NewItem("C")];

        Assert.Equal(3, Search(items, search).Count);
    }

    [Fact]
    public void C07_Search_ignores_surrounding_whitespace()
    {
        var invoice = NewItem("Invoice batch");

        Assert.Equal([invoice], Search([invoice, NewItem("Other")], "  invoice "));
    }

    [Fact]
    public void C08_Search_is_the_same_in_every_culture()
    {
        var index = NewItem("SEARCH INDEX", externalId: null);
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Assert.Equal([index], Search([index, NewItem("Other")], "index"));
            Assert.Equal([index], Search([index, NewItem("Other")], "Search Index"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void C09_Sorting_by_name_ignores_case_in_both_directions()
    {
        var alpha = NewItem("alpha");
        var bravo = NewItem("Bravo");
        var charlie = NewItem("charlie");

        var ascending = Sort([charlie, alpha, bravo], ItemSortField.Name, SortDirection.Ascending);
        var descending = Sort([charlie, alpha, bravo], ItemSortField.Name, SortDirection.Descending);

        Assert.Equal([alpha, bravo, charlie], ascending);
        Assert.Equal([charlie, bravo, alpha], descending);
    }

    [Fact]
    public void C10_Sorting_by_status_and_creation_time_in_both_directions()
    {
        var draft = NewItem("D", status: ItemStatus.Draft, createdMinutesAgo: 10);
        var archived = NewItem("A", status: ItemStatus.Archived, createdMinutesAgo: 20);

        Assert.Equal([draft, archived], Sort([archived, draft], ItemSortField.Status, SortDirection.Ascending));
        Assert.Equal([archived, draft], Sort([draft, archived], ItemSortField.Status, SortDirection.Descending));
        Assert.Equal([archived, draft], Sort([draft, archived], ItemSortField.CreatedAt, SortDirection.Ascending));
        Assert.Equal([draft, archived], Sort([archived, draft], ItemSortField.CreatedAt, SortDirection.Descending));
    }

    [Fact]
    public void C11_Items_without_an_external_id_always_come_last()
    {
        var first = NewItem("First", externalId: "a-001");
        var second = NewItem("Second", externalId: "B-002");
        var missing = NewItem("Missing", externalId: null);

        var ascending = Sort([missing, second, first], ItemSortField.ExternalId, SortDirection.Ascending);
        var descending = Sort([missing, first, second], ItemSortField.ExternalId, SortDirection.Descending);

        Assert.Equal([first, second, missing], ascending);
        Assert.Equal([second, first, missing], descending);
    }

    [Fact]
    public void C12_Pages_split_the_results_in_order()
    {
        var items = Enumerable.Range(0, 45).Select(i => NewItem($"Item {i:D2}", createdMinutesAgo: i)).ToList();

        var second = ItemQuery.Execute(items, new ItemQueryOptions { Page = 2, PageSize = 20 });
        var last = ItemQuery.Execute(items, new ItemQueryOptions { Page = 3, PageSize = 20 });

        Assert.Equal(items.Skip(20).Take(20), second.Items);
        Assert.Equal(new ItemPage(second.Items, TotalCount: 45, Page: 2, PageSize: 20, TotalPages: 3), second);
        Assert.Equal(items.Skip(40), last.Items);
        Assert.Equal(new ItemPage(last.Items, TotalCount: 45, Page: 3, PageSize: 20, TotalPages: 3), last);
    }

    [Fact]
    public void C13_Totals_count_the_search_results_not_the_whole_source()
    {
        var items = Enumerable.Range(0, 30)
            .Select(i => NewItem(i % 3 == 0 ? $"Report {i}" : $"Invoice {i}", createdMinutesAgo: i))
            .ToList();

        var page = ItemQuery.Execute(items, new ItemQueryOptions { Search = "report", PageSize = 4 });

        Assert.Equal(10, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(4, page.Items.Count);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(int.MaxValue)]
    public void C14_A_page_past_the_end_is_empty_but_keeps_the_totals(int pageNumber)
    {
        var items = Enumerable.Range(0, 5).Select(i => NewItem($"Item {i}")).ToList();

        var page = ItemQuery.Execute(items, new ItemQueryOptions { Page = pageNumber, PageSize = 2 });

        Assert.Empty(page.Items);
        Assert.Equal(new ItemPage(page.Items, TotalCount: 5, Page: pageNumber, PageSize: 2, TotalPages: 3), page);
    }

    [Theory]
    [InlineData(ItemSortField.Name)]
    [InlineData(ItemSortField.ExternalId)]
    [InlineData(ItemSortField.Status)]
    [InlineData(ItemSortField.CreatedAt)]
    public void C15_Equal_sort_keys_produce_the_same_order_whatever_the_input_order(ItemSortField sortBy)
    {
        var items = Enumerable.Range(0, 12).Select(i => NewItem("Same name", createdMinutesAgo: 5)).ToList();
        var reversed = Enumerable.Reverse(items).ToList();
        var options = new ItemQueryOptions { SortBy = sortBy, PageSize = 5 };

        for (var pageNumber = 1; pageNumber <= 3; pageNumber++)
        {
            var fromOriginal = ItemQuery.Execute(items, options with { Page = pageNumber });
            var fromReversed = ItemQuery.Execute(reversed, options with { Page = pageNumber });

            Assert.Equal(fromOriginal.Items, fromReversed.Items);
        }
    }

    [Fact]
    public void C16_The_source_is_enumerated_only_once()
    {
        var source = new SinglePassEnumerable([NewItem("Report"), NewItem("Invoice"), NewItem("Report draft")]);

        var page = ItemQuery.Execute(source, new ItemQueryOptions { Search = "report", PageSize = 1 });

        Assert.Equal(2, page.TotalCount);
        Assert.Single(page.Items);
    }

    private static List<Item> Search(IEnumerable<Item> items, string? search) =>
        [.. ItemQuery.Execute(items, new ItemQueryOptions { Search = search }).Items];

    private static List<Item> Sort(IEnumerable<Item> items, ItemSortField field, SortDirection direction) =>
        [.. ItemQuery.Execute(items, new ItemQueryOptions { SortBy = field, Direction = direction }).Items];

    private static Item NewItem(
        string name,
        string? externalId = null,
        ItemStatus status = ItemStatus.Active,
        int createdMinutesAgo = 0) =>
        new(Guid.NewGuid(), name, externalId, status, Now.AddMinutes(-createdMinutesAgo));

    private sealed class SinglePassEnumerable(IEnumerable<Item> items) : IEnumerable<Item>
    {
        private bool _enumerated;

        public IEnumerator<Item> GetEnumerator()
        {
            Assert.False(_enumerated, "The source sequence was enumerated more than once.");
            _enumerated = true;
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
