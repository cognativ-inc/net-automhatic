using AutoMHatic.Assessment.Exercise02;

namespace AutoMHatic.Assessment.Tests.Exercise02;

public sealed class ReviewProgressTests
{
    [Fact]
    public void C01_Null_fields_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => ReviewProgress.Calculate(null!, hasCoBorrower: false));
    }

    [Fact]
    public void C02_Nothing_required_means_complete()
    {
        Assert.Equal(100, ReviewProgress.Calculate([], hasCoBorrower: false));
        Assert.Equal(100, ReviewProgress.Calculate([Optional("notes", null)], hasCoBorrower: false));
    }

    [Fact]
    public void C03_All_or_none_of_the_required_fields_filled()
    {
        Assert.Equal(100, ReviewProgress.Calculate([Required("a", "x"), Required("b", "y")], hasCoBorrower: false));
        Assert.Equal(0, ReviewProgress.Calculate([Required("a", null), Required("b", null)], hasCoBorrower: false));
    }

    [Fact]
    public void C04_Optional_fields_do_not_affect_progress()
    {
        ReviewField[] fields =
        [
            Required("firstName", "Ada"),
            Required("lastName", null),
            Optional("middleName", "King"),
            Optional("suffix", null),
        ];

        Assert.Equal(50, ReviewProgress.Calculate(fields, hasCoBorrower: false));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    public void C05_Blank_values_count_as_missing(string blank)
    {
        Assert.Equal(50, ReviewProgress.Calculate([Required("a", "x"), Required("b", blank)], hasCoBorrower: false));
    }

    [Fact]
    public void C06_Partial_progress_rounds_up()
    {
        Assert.Equal(34, ReviewProgress.Calculate(RequiredFields(total: 3, filled: 1), hasCoBorrower: false));
        Assert.Equal(67, ReviewProgress.Calculate(RequiredFields(total: 3, filled: 2), hasCoBorrower: false));
        Assert.Equal(1, ReviewProgress.Calculate(RequiredFields(total: 1000, filled: 1), hasCoBorrower: false));
    }

    [Theory]
    [InlineData(100, 7, 7)]
    [InlineData(100, 14, 14)]
    [InlineData(100, 28, 28)]
    [InlineData(100, 56, 56)]
    [InlineData(20, 11, 55)]
    [InlineData(50, 7, 14)]
    [InlineData(25, 7, 28)]
    public void C07_Exact_percentages_are_not_rounded_up(int total, int filled, int expected)
    {
        Assert.Equal(expected, ReviewProgress.Calculate(RequiredFields(total, filled), hasCoBorrower: false));
    }

    [Theory]
    [InlineData(200, 199)]
    [InlineData(1000, 991)]
    [InlineData(1000, 999)]
    public void C08_Progress_never_reports_100_while_a_required_field_is_missing(int total, int filled)
    {
        Assert.Equal(99, ReviewProgress.Calculate(RequiredFields(total, filled), hasCoBorrower: false));
    }

    [Fact]
    public void C09_Co_borrower_fields_are_required_only_with_a_co_borrower()
    {
        ReviewField[] fields =
        [
            Required("borrowerName", "Ada"),
            new("coBorrowerName", null, IsRequired: true, IsCoBorrowerField: true),
            new("coBorrowerNickname", null, IsRequired: false, IsCoBorrowerField: true),
        ];

        Assert.Equal(100, ReviewProgress.Calculate(fields, hasCoBorrower: false));
        Assert.Equal(50, ReviewProgress.Calculate(fields, hasCoBorrower: true));
    }

    [Fact]
    public void C10_The_last_occurrence_of_a_key_wins()
    {
        ReviewField[] laterFilled = [Required("ssn", null), Required("dob", "01/02/1980"), Required("ssn", "123456789")];
        ReviewField[] laterCleared = [Required("ssn", "123456789"), Required("dob", "01/02/1980"), Required("ssn", " ")];

        Assert.Equal(100, ReviewProgress.Calculate(laterFilled, hasCoBorrower: false));
        Assert.Equal(50, ReviewProgress.Calculate(laterCleared, hasCoBorrower: false));
    }

    [Fact]
    public void C11_A_later_occurrence_can_change_whether_a_key_is_required()
    {
        ReviewField[] fields = [Required("employer", null), Required("income", "5000"), Optional("employer", null)];

        Assert.Equal(100, ReviewProgress.Calculate(fields, hasCoBorrower: false));
    }

    private static ReviewField Required(string key, string? value) => new(key, value, IsRequired: true);

    private static ReviewField Optional(string key, string? value) => new(key, value, IsRequired: false);

    private static IEnumerable<ReviewField> RequiredFields(int total, int filled) =>
        Enumerable.Range(0, total).Select(i => Required($"field{i}", i < filled ? "value" : null));
}
