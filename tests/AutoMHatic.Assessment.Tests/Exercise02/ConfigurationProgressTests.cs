using AutoMHatic.Assessment.Exercise02;

namespace AutoMHatic.Assessment.Tests.Exercise02;

public sealed class ConfigurationProgressTests
{
    [Fact]
    public void C01_Null_settings_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => ConfigurationProgress.Calculate(null!));
    }

    [Fact]
    public void C02_No_required_settings_means_complete()
    {
        Assert.Equal(100, ConfigurationProgress.Calculate([]));
        Assert.Equal(100, ConfigurationProgress.Calculate([Optional("theme", null)]));
    }

    [Fact]
    public void C03_All_or_none_of_the_required_settings_have_a_value()
    {
        Assert.Equal(100, ConfigurationProgress.Calculate([Required("a", "x"), Required("b", "y")]));
        Assert.Equal(0, ConfigurationProgress.Calculate([Required("a", null), Required("b", null)]));
    }

    [Fact]
    public void C04_Optional_settings_do_not_affect_the_result()
    {
        Setting[] settings =
        [
            Required("host", "localhost"),
            Required("port", null),
            Optional("timeout", "30"),
            Optional("proxy", null),
        ];

        Assert.Equal(50, ConfigurationProgress.Calculate(settings));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    public void C05_Blank_values_count_as_missing(string blank)
    {
        Assert.Equal(50, ConfigurationProgress.Calculate([Required("a", "x"), Required("b", blank)]));
    }

    [Fact]
    public void C06_Partial_percentages_round_up()
    {
        Assert.Equal(34, ConfigurationProgress.Calculate(RequiredSettings(total: 3, filled: 1)));
        Assert.Equal(67, ConfigurationProgress.Calculate(RequiredSettings(total: 3, filled: 2)));
        Assert.Equal(1, ConfigurationProgress.Calculate(RequiredSettings(total: 1000, filled: 1)));
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
        Assert.Equal(expected, ConfigurationProgress.Calculate(RequiredSettings(total, filled)));
    }

    [Theory]
    [InlineData(200, 199)]
    [InlineData(1000, 991)]
    [InlineData(1000, 999)]
    public void C08_The_result_is_never_100_while_a_required_setting_is_missing(int total, int filled)
    {
        Assert.Equal(99, ConfigurationProgress.Calculate(RequiredSettings(total, filled)));
    }

    [Fact]
    public void C09_The_last_occurrence_of_a_key_wins()
    {
        Setting[] laterFilled = [Required("host", null), Required("port", "8080"), Required("host", "localhost")];
        Setting[] laterCleared = [Required("host", "localhost"), Required("port", "8080"), Required("host", " ")];

        Assert.Equal(100, ConfigurationProgress.Calculate(laterFilled));
        Assert.Equal(50, ConfigurationProgress.Calculate(laterCleared));
    }

    [Fact]
    public void C10_A_later_occurrence_can_change_whether_a_key_is_required()
    {
        Setting[] settings = [Required("proxy", null), Required("host", "localhost"), Optional("proxy", null)];

        Assert.Equal(100, ConfigurationProgress.Calculate(settings));
    }

    [Fact]
    public void C11_Keys_are_case_sensitive()
    {
        Setting[] settings = [Required("Host", null), Required("host", "localhost")];

        Assert.Equal(50, ConfigurationProgress.Calculate(settings));
    }

    private static Setting Required(string key, string? value) => new(key, value, IsRequired: true);

    private static Setting Optional(string key, string? value) => new(key, value, IsRequired: false);

    private static IEnumerable<Setting> RequiredSettings(int total, int filled) =>
        Enumerable.Range(0, total).Select(i => Required($"setting{i}", i < filled ? "value" : null));
}
