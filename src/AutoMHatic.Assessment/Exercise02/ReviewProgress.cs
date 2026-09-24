/*
 * Exercise 2 - Review progress indicator
 * Difficulty: easy  |  Suggested time: 6 minutes
 *
 * Premise
 * -------
 * In the Record Review Workbench, a Data Entry operator checks the fields that
 * the extraction pipeline produced for a loan application. A progress
 * indicator shows how close the application is to being submittable. The
 * product specification defines progress like this:
 *
 *     progress = ceil( required_fields_with_a_value / total_required_fields * 100 )
 *
 * Operators rely on this number. When they see 100, they press Submit.
 *
 * Your task
 * ---------
 * Implement ReviewProgress.Calculate. It returns an integer from 0 to 100.
 *
 * - Only required fields affect progress.
 * - Co-borrower fields are required only when the application has a
 *   co-borrower.
 * - When nothing is required, nothing blocks submission.
 * - The extraction pipeline can emit the same field more than once when it
 *   re-reads a page. The last occurrence of a key is the current one.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise02/ReviewProgressTests.cs
 */

namespace AutoMHatic.Assessment.Exercise02;

public sealed record ReviewField(string Key, string? Value, bool IsRequired, bool IsCoBorrowerField = false);

public static class ReviewProgress
{
    public static int Calculate(IEnumerable<ReviewField> fields, bool hasCoBorrower)
    {
        throw new NotImplementedException();
    }
}
