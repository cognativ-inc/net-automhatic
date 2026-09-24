/*
 * Exercise 5 - Submitting to Encompass
 * Difficulty: hard  |  Suggested time: 12 minutes
 *
 * Premise
 * -------
 * When an application is ready, the Lender Portal creates its loan file in
 * Encompass, the Loan Origination System. Encompass has no idempotency key. If
 * it is called twice for the same application, it creates two loan files, and
 * someone in operations has to clean up a duplicate loan by hand.
 *
 * Several things can trigger a submission at the same time: the operator's
 * Submit button (often double-clicked), a supervisor's bulk action, and a
 * background retry job. They all call EncompassSubmitter, which is registered
 * as a singleton.
 *
 * Encompass fails in two ways:
 * - EncompassTransientException: a timeout or throttling. Trying again usually
 *   works. Make at most 3 attempts in total. No back-off is required.
 * - EncompassRejectedException: the data is wrong. Trying again cannot help
 *   until an operator fixes the application, and then they submit again.
 *
 * Your task
 * ---------
 * Implement EncompassSubmitter.SubmitAsync. It returns the Encompass loan ID
 * for the application.
 *
 * - The constructor throws ArgumentNullException for a null client.
 *   SubmitAsync throws ArgumentException for an empty application ID.
 * - Encompass must never create two loan files for the same application.
 * - Callers that submit the same application at the same time get the same
 *   outcome.
 * - Submissions for different applications must not wait for each other.
 * - The cancellation token means that this caller stops waiting. It must not
 *   abort work that other callers depend on. A caller whose token is already
 *   cancelled gets an OperationCanceledException and never reaches Encompass.
 *
 * You may add private members and helper types. Keep the public surface.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise05/EncompassSubmitterTests.cs
 */

namespace AutoMHatic.Assessment.Exercise05;

public interface IEncompassClient
{
    Task<string> CreateLoanFileAsync(Guid applicationId, CancellationToken cancellationToken);
}

public sealed class EncompassTransientException(string message) : Exception(message);

public sealed class EncompassRejectedException(string message) : Exception(message);

public sealed class EncompassSubmitter
{
    public EncompassSubmitter(IEncompassClient client)
    {
        throw new NotImplementedException();
    }

    public Task<string> SubmitAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
