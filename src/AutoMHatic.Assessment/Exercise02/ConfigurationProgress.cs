/*
 * Exercise 2 - Configuration completeness
 * Difficulty: easy  |  Suggested time: 6 minutes
 *
 * A settings screen shows how complete a configuration is: the percentage of
 * its required settings that have a value. Users treat 100 as "done".
 *
 * Implement ConfigurationProgress.Calculate. It returns an integer from 0 to 100.
 *
 * - Throw ArgumentNullException when settings is null.
 * - Only required settings count.
 * - A null, empty, or whitespace-only value counts as missing.
 * - A key can appear more than once. The last occurrence of a key replaces the
 *   earlier ones. Keys are case-sensitive.
 * - With no required settings, return 100.
 * - Round partial percentages up. For example, 1 of 3 is 34.
 * - Never return 100 while a required setting is missing.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise02/ConfigurationProgressTests.cs
 */

namespace AutoMHatic.Assessment.Exercise02;

public sealed record Setting(string Key, string? Value, bool IsRequired);

public static class ConfigurationProgress
{
    public static int Calculate(IEnumerable<Setting> settings)
    {
        throw new NotImplementedException();
    }
}
