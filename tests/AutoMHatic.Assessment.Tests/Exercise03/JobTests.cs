using AutoMHatic.Assessment.Exercise03;
using static AutoMHatic.Assessment.Exercise03.JobStatus;

namespace AutoMHatic.Assessment.Tests.Exercise03;

public sealed class JobTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 24, 9, 0, 0, TimeSpan.Zero);

    private readonly ManualTimeProvider _clock = new(Start);

    [Fact]
    public void C01_Constructor_rejects_invalid_arguments()
    {
        Assert.ThrowsAny<ArgumentException>(() => new Job(Guid.Empty, _clock));
        Assert.Throws<ArgumentNullException>(() => new Job(Guid.NewGuid(), null!));
    }

    [Fact]
    public void C02_A_new_job_is_pending_with_no_history()
    {
        var id = Guid.NewGuid();

        var job = new Job(id, _clock);

        Assert.Equal(id, job.Id);
        Assert.Equal(Pending, job.Status);
        Assert.Empty(job.History);
    }

    [Fact]
    public void C03_A_job_can_run_to_completion()
    {
        var job = NewJob();

        MoveThrough(job, Running, Completed);

        Assert.Equal(Completed, job.Status);
        Assert.Equal(2, job.History.Count);
    }

    [Fact]
    public void C04_Every_allowed_transition_succeeds()
    {
        MoveThrough(NewJob(), Cancelled);
        MoveThrough(NewJob(), Running, Cancelled);
        MoveThrough(NewJob(), Running, Failed, Pending, Running, Completed);
    }

    [Fact]
    public void C05_CanMoveTo_answers_without_changing_state()
    {
        var job = NewJob();

        Assert.True(job.CanMoveTo(Running));
        Assert.True(job.CanMoveTo(Cancelled));
        Assert.False(job.CanMoveTo(Completed));
        Assert.False(job.CanMoveTo(Failed));
        Assert.Equal(Pending, job.Status);
        Assert.Empty(job.History);
    }

    [Theory]
    [InlineData(new[] { Completed })]
    [InlineData(new[] { Failed })]
    [InlineData(new[] { Running, Pending })]
    [InlineData(new[] { Running, Failed, Running })]
    [InlineData(new[] { Running, Failed, Cancelled })]
    public void C06_A_disallowed_transition_throws_and_changes_nothing(JobStatus[] path)
    {
        var job = NewJob();
        var allowed = path[..^1];
        var disallowed = path[^1];
        MoveThrough(job, allowed);

        Assert.False(job.CanMoveTo(disallowed));
        Assert.Throws<InvalidOperationException>(() => job.MoveTo(disallowed));

        Assert.Equal(allowed.Length == 0 ? Pending : allowed[^1], job.Status);
        Assert.Equal(allowed.Length, job.History.Count);
    }

    [Theory]
    [InlineData(Pending)]
    [InlineData(Running)]
    [InlineData(Failed)]
    public void C07_Moving_to_the_current_status_is_not_a_transition(JobStatus status)
    {
        var job = NewJob();
        MoveThrough(job, PathTo(status));

        Assert.False(job.CanMoveTo(status));
        Assert.Throws<InvalidOperationException>(() => job.MoveTo(status));
    }

    [Theory]
    [InlineData(Completed)]
    [InlineData(Cancelled)]
    public void C08_Completed_and_cancelled_jobs_are_final(JobStatus final)
    {
        var job = NewJob();
        MoveThrough(job, PathTo(final));

        foreach (var status in Enum.GetValues<JobStatus>())
        {
            Assert.False(job.CanMoveTo(status), $"{final} must not move to {status}.");
            Assert.Throws<InvalidOperationException>(() => job.MoveTo(status));
        }
    }

    [Fact]
    public void C09_Undefined_statuses_are_rejected()
    {
        var job = NewJob();
        var undefined = (JobStatus)999;

        Assert.False(job.CanMoveTo(undefined));
        var error = Assert.ThrowsAny<Exception>(() => job.MoveTo(undefined));
        Assert.True(
            error is InvalidOperationException or ArgumentException,
            $"Expected an InvalidOperationException, got {error.GetType().Name}.");
        Assert.Equal(Pending, job.Status);
        Assert.Empty(job.History);
    }

    [Fact]
    public void C10_History_records_each_change_with_the_time_from_the_TimeProvider()
    {
        var job = NewJob();

        job.MoveTo(Running);
        _clock.Advance(TimeSpan.FromMinutes(3));
        job.MoveTo(Failed);
        _clock.Advance(TimeSpan.FromHours(1));
        job.MoveTo(Pending);

        Assert.Equal(
            [
                new StatusChange(Pending, Running, Start),
                new StatusChange(Running, Failed, Start.AddMinutes(3)),
                new StatusChange(Failed, Pending, Start.AddMinutes(63)),
            ],
            job.History);
    }

    [Fact]
    public void C11_Callers_cannot_change_the_history()
    {
        var job = NewJob();
        job.MoveTo(Running);

        // Returning a read-only view or a fresh copy are both fine. What matters is that
        // tampering with what History returns never changes the job's own record.
        if (job.History is IList<StatusChange> list)
        {
            TryToTamper(() => list[0] = new StatusChange(Pending, Cancelled, Start));
            TryToTamper(() => list.Add(new StatusChange(Running, Completed, Start)));
            TryToTamper(list.Clear);
        }

        Assert.Equal(new StatusChange(Pending, Running, Start), Assert.Single(job.History));
    }

    [Fact]
    public void C12_A_failed_job_can_be_retried_at_most_three_times()
    {
        var job = NewJob();
        MoveThrough(job, Running, Failed);

        for (var retry = 1; retry <= 3; retry++)
        {
            Assert.True(job.CanMoveTo(Pending), $"Retry {retry} should be allowed.");
            MoveThrough(job, Pending, Running, Failed);
        }

        Assert.False(job.CanMoveTo(Pending));
        Assert.Throws<InvalidOperationException>(() => job.MoveTo(Pending));
        Assert.Equal(Failed, job.Status);
        Assert.Equal(11, job.History.Count);
    }

    [Fact]
    public void C13_Each_job_has_its_own_retry_limit()
    {
        var first = NewJob();
        var second = NewJob();
        MoveThrough(first, Running, Failed);
        MoveThrough(second, Running, Failed);

        for (var retry = 1; retry <= 3; retry++)
        {
            MoveThrough(first, Pending, Running, Failed);
        }

        Assert.False(first.CanMoveTo(Pending));
        Assert.True(second.CanMoveTo(Pending));
    }

    private Job NewJob() => new(Guid.NewGuid(), _clock);

    private static void TryToTamper(Action tamper)
    {
        try
        {
            tamper();
        }
        catch (NotSupportedException)
        {
            // Read-only collections refuse the change, which is fine.
        }
    }

    private static void MoveThrough(Job job, params JobStatus[] statuses)
    {
        foreach (var status in statuses)
        {
            job.MoveTo(status);
        }
    }

    private static JobStatus[] PathTo(JobStatus status) => status switch
    {
        Pending => [],
        Running => [Running],
        Completed => [Running, Completed],
        Failed => [Running, Failed],
        Cancelled => [Cancelled],
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}
