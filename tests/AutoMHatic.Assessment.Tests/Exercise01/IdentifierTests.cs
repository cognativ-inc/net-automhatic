using System.Globalization;
using AutoMHatic.Assessment.Exercise01;

namespace AutoMHatic.Assessment.Tests.Exercise01;

public sealed class IdentifierTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void C01_Missing_input_returns_null(string? input)
    {
        Assert.Null(Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("abc123")]
    [InlineData("hello-world")]
    public void C02_An_identifier_that_is_already_normalized_is_unchanged(string input)
    {
        Assert.Equal(input, Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("  hello")]
    [InlineData("hello  ")]
    [InlineData("\thello\r\n")]
    public void C03_Surrounding_whitespace_is_ignored(string input)
    {
        Assert.Equal("hello", Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("HELLO", "hello")]
    [InlineData("HeLLo123", "hello123")]
    public void C04_Letters_are_lowercased(string input, string expected)
    {
        Assert.Equal(expected, Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("hello world", "hello-world")]
    [InlineData("hello_world", "hello-world")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("user_123", "user-123")]
    public void C05_Spaces_and_underscores_become_hyphens(string input, string expected)
    {
        Assert.Equal(expected, Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("foo---bar", "foo-bar")]
    [InlineData("USER__123", "user-123")]
    [InlineData("a _ - b", "a-b")]
    [InlineData("  Hello   World ", "hello-world")]
    public void C06_Repeated_separators_collapse_into_one_hyphen(string input, string expected)
    {
        Assert.Equal(expected, Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("-foo")]
    [InlineData("foo-")]
    [InlineData("_foo_")]
    [InlineData("- _foo_ -")]
    public void C07_Leading_and_trailing_separators_are_dropped(string input)
    {
        Assert.Equal("foo", Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("-")]
    [InlineData("___")]
    [InlineData(" - _ - ")]
    public void C08_Input_made_only_of_separators_returns_null(string input)
    {
        Assert.Null(Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("hello@world")]
    [InlineData("foo.bar")]
    [InlineData("a/b")]
    [InlineData("tab\tinside")]
    [InlineData("café")]
    [InlineData("１２３")]
    public void C09_Input_with_a_disallowed_character_returns_null(string input)
    {
        Assert.Null(Identifier.Normalize(input));
    }

    [Theory]
    [InlineData("TITLE", "title")]
    [InlineData("ID_LIST", "id-list")]
    [InlineData("Item Index", "item-index")]
    public void C10_The_result_is_the_same_in_every_culture(string input, string expected)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Assert.Equal(expected, Identifier.Normalize(input));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
