/*
 * Exercise 3 - Job state machine
 * Difficulty: medium  |  Suggested time: 8 minutes
 *
 * A background job moves through these statuses. Only these transitions exist:
 *
 *     Pending    ->  Running, Cancelled
 *     Running    ->  Completed, Failed, Cancelled
 *     Failed     ->  Pending   (a retry)
 *     Completed and Cancelled are final.
 *
 * Implement Job.
 *
 * - The constructor throws ArgumentException for an empty ID and
 *   ArgumentNullException for a null TimeProvider.
 * - A new job is Pending with an empty history.
 * - MoveTo applies a transition and adds it to History, timestamped with the
 *   TimeProvider.
 * - Any other transition, including one to an undefined status, throws
 *   InvalidOperationException and changes nothing.
 * - CanMoveTo answers whether MoveTo would succeed, without changing anything.
 * - Callers must not be able to change History.
 * - A job can be retried at most 3 times.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise03/JobTests.cs
 */

namespace AutoMHatic.Assessment.Exercise03;

public enum JobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
}

public sealed record StatusChange(JobStatus From, JobStatus To, DateTimeOffset At);

public sealed class Job
{
    public Job(Guid id, TimeProvider timeProvider)
    {
        throw new NotImplementedException();
    }

    public Guid Id { get; }

    public JobStatus Status { get; }

    public IReadOnlyList<StatusChange> History { get; } = [];

    public bool CanMoveTo(JobStatus next)
    {
        throw new NotImplementedException();
    }

    public void MoveTo(JobStatus next)
    {
        throw new NotImplementedException();
    }
}
