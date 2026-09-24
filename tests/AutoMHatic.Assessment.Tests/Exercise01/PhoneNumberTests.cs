using AutoMHatic.Assessment.Exercise01;

namespace AutoMHatic.Assessment.Tests.Exercise01;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void C01_Missing_input_returns_null(string? input)
    {
        Assert.Null(PhoneNumber.Normalize(input));
    }

    [Fact]
    public void C02_Ten_plain_digits_are_already_canonical()
    {
        Assert.Equal("5551234567", PhoneNumber.Normalize("5551234567"));
    }

    [Fact]
    public void C03_Surrounding_whitespace_is_ignored()
    {
        Assert.Equal("5551234567", PhoneNumber.Normalize("  5551234567 \t"));
    }

    [Theory]
    [InlineData("(555) 123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("555.123.4567")]
    [InlineData("555 123 4567")]
    [InlineData("(555)123-4567")]
    public void C04_Common_grouping_characters_are_removed(string input)
    {
        Assert.Equal("5551234567", PhoneNumber.Normalize(input));
    }

    [Theory]
    [InlineData("555123456")]
    [InlineData("555-123-456")]
    [InlineData("55512345678")]
    [InlineData("25551234567")]
    public void C05_Wrong_number_of_digits_returns_null(string input)
    {
        Assert.Null(PhoneNumber.Normalize(input));
    }

    [Theory]
    [InlineData("555-CALL-NOW")]
    [InlineData("555_123_4567")]
    [InlineData("555/123/4567")]
    [InlineData("555-123-4567 ext 12")]
    [InlineData("555#1234567")]
    public void C06_Unexpected_characters_return_null(string input)
    {
        Assert.Null(PhoneNumber.Normalize(input));
    }

    [Theory]
    [InlineData("+1 555 123 4567")]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("+15551234567")]
    [InlineData("1-555-123-4567")]
    [InlineData("15551234567")]
    public void C07_US_country_code_is_removed(string input)
    {
        Assert.Equal("5551234567", PhoneNumber.Normalize(input));
    }

    [Theory]
    [InlineData("+5551234567")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("555+1234567")]
    [InlineData("++15551234567")]
    public void C08_Non_US_or_misplaced_country_codes_return_null(string input)
    {
        Assert.Null(PhoneNumber.Normalize(input));
    }

    [Theory]
    [InlineData("0000000000")]
    [InlineData("000-000-0000")]
    [InlineData("+1 000 000 0000")]
    public void C09_All_zero_placeholders_return_null(string input)
    {
        Assert.Null(PhoneNumber.Normalize(input));
    }
}
