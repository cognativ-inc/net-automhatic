/*
 * Exercise 1 - Phone number normalization
 * Difficulty: very easy  |  Suggested time: 4 minutes
 *
 * Premise
 * -------
 * Loan applications reach the Lender Portal as scanned or emailed forms. The
 * extraction pipeline and the Data Entry operators type phone numbers in any
 * shape they find on the page. Before a loan file is created in Encompass,
 * every phone number must be stored in one canonical form: exactly 10 digits,
 * with no formatting.
 *
 * Your task
 * ---------
 * Implement PhoneNumber.Normalize.
 *
 * - Return the canonical 10-digit string when the input is a valid US phone
 *   number.
 * - Return null when the input cannot be a valid US phone number. Do not throw.
 * - People group digits with spaces, dashes, dots, and parentheses.
 * - People sometimes include the US country code.
 * - A number made only of zeros is a placeholder, not a phone number.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise01/PhoneNumberTests.cs
 */

namespace AutoMHatic.Assessment.Exercise01;

public static class PhoneNumber
{
    public static string? Normalize(string? input)
    {
        throw new NotImplementedException();
    }
}
