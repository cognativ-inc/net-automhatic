/*
 * Exercise 3 - Application lifecycle
 * Difficulty: medium  |  Suggested time: 8 minutes
 *
 * Premise
 * -------
 * Every loan application moves through a fixed set of business statuses, from
 * the moment its email arrives to the moment its loan file exists in Encompass.
 * Supervisors audit this lifecycle, so every change must be recorded with the
 * time it happened. The product team agreed on these transitions:
 *
 *     Received            -> NeedsAssignment, IntakeIncomplete, Processing
 *     NeedsAssignment     -> Processing
 *     IntakeIncomplete    -> Processing
 *     Processing          -> ReviewRequired
 *     ReviewRequired      -> ReviewInProgress, Cancelled
 *     ReviewInProgress    -> ReviewRequired, ReadyForSubmission, Cancelled
 *     ReadyForSubmission  -> SubmittedToEncompass, SubmissionFailed
 *     SubmissionFailed    -> ReadyForSubmission   (a retry)
 *
 * Encompass throttles lenders that hammer it, so an application may be retried
 * at most 3 times. After that, a supervisor must look at it.
 *
 * Your task
 * ---------
 * Implement LoanApplication.
 *
 * - A new application starts in Received with an empty history.
 * - MoveTo applies a transition. A transition that is not allowed must leave the
 *   application untouched.
 * - CanMoveTo answers whether MoveTo would succeed, without changing anything.
 * - History exposes every change in the order it happened. Callers must not be
 *   able to rewrite it.
 * - Use the TimeProvider for timestamps.
 *
 * You may add private members, change the shape of the class body, and add new
 * types. Keep the public members that the tests use.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise03/LoanApplicationTests.cs
 */

namespace AutoMHatic.Assessment.Exercise03;

public enum ApplicationStatus
{
    Received,
    NeedsAssignment,
    IntakeIncomplete,
    Processing,
    ReviewRequired,
    ReviewInProgress,
    ReadyForSubmission,
    SubmittedToEncompass,
    SubmissionFailed,
    Cancelled,
}

public sealed record StatusChange(ApplicationStatus From, ApplicationStatus To, DateTimeOffset At);

public sealed class LoanApplication
{
    public LoanApplication(Guid id, TimeProvider timeProvider)
    {
        throw new NotImplementedException();
    }

    public Guid Id { get; }

    public ApplicationStatus Status { get; }

    public IReadOnlyList<StatusChange> History { get; } = [];

    public bool CanMoveTo(ApplicationStatus next)
    {
        throw new NotImplementedException();
    }

    public void MoveTo(ApplicationStatus next)
    {
        throw new NotImplementedException();
    }
}
