/*
 * Exercise 1 - Identifier normalization
 * Difficulty: very easy  |  Suggested time: 4 minutes
 *
 * Turn free-form text into a clean identifier.
 *
 * Implement Identifier.Normalize.
 *
 * - Return null for null, empty, or whitespace-only input.
 * - Ignore surrounding whitespace.
 * - Lowercase all letters. The result must be the same in every culture.
 * - A space, an underscore, or a hyphen is a separator. Each run of separators
 *   becomes a single "-".
 * - Drop separators at the start and at the end.
 * - The result may contain only a-z, 0-9, and "-". Return null when the input
 *   contains any other character, or when nothing is left. Never throw.
 *
 * Examples:
 *
 *     "  Hello World "  ->  "hello-world"
 *     "USER__123"       ->  "user-123"
 *     "foo---bar"       ->  "foo-bar"
 *     "_draft_"         ->  "draft"
 *     "hello@world"     ->  null
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise01/IdentifierTests.cs
 */

namespace AutoMHatic.Assessment.Exercise01;

public static class Identifier
{
    public static string? Normalize(string? input)
    {
        throw new NotImplementedException();
    }
}
