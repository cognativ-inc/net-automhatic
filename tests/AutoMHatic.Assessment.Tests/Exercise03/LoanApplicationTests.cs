using AutoMHatic.Assessment.Exercise03;
using static AutoMHatic.Assessment.Exercise03.ApplicationStatus;

namespace AutoMHatic.Assessment.Tests.Exercise03;

public sealed class LoanApplicationTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 24, 9, 0, 0, TimeSpan.Zero);

    private readonly ManualTimeProvider _clock = new(Start);

    [Fact]
    public void C01_Constructor_rejects_invalid_arguments()
    {
        Assert.ThrowsAny<ArgumentException>(() => new LoanApplication(Guid.Empty, _clock));
        Assert.Throws<ArgumentNullException>(() => new LoanApplication(Guid.NewGuid(), null!));
    }

    [Fact]
    public void C02_A_new_application_is_received_with_no_history()
    {
        var id = Guid.NewGuid();

        var application = new LoanApplication(id, _clock);

        Assert.Equal(id, application.Id);
        Assert.Equal(Received, application.Status);
        Assert.Empty(application.History);
    }

    [Fact]
    public void C03_The_happy_path_reaches_Encompass()
    {
        var application = NewApplication();

        MoveThrough(application, Processing, ReviewRequired, ReviewInProgress, ReadyForSubmission, SubmittedToEncompass);

        Assert.Equal(SubmittedToEncompass, application.Status);
        Assert.Equal(5, application.History.Count);
    }

    [Fact]
    public void C04_Every_documented_path_is_allowed()
    {
        MoveThrough(NewApplication(), NeedsAssignment, Processing);
        MoveThrough(NewApplication(), IntakeIncomplete, Processing);
        MoveThrough(NewApplication(), Processing, ReviewRequired, ReviewInProgress, ReviewRequired, ReviewInProgress);
        MoveThrough(NewApplication(), Processing, ReviewRequired, ReviewInProgress, ReadyForSubmission, SubmissionFailed);
    }

    [Fact]
    public void C05_CanMoveTo_answers_without_changing_state()
    {
        var application = NewApplication();

        Assert.True(application.CanMoveTo(Processing));
        Assert.True(application.CanMoveTo(NeedsAssignment));
        Assert.False(application.CanMoveTo(ReviewRequired));
        Assert.False(application.CanMoveTo(SubmittedToEncompass));
        Assert.Equal(Received, application.Status);
        Assert.Empty(application.History);
    }

    [Fact]
    public void C06_A_disallowed_transition_throws_and_changes_nothing()
    {
        var application = NewApplication();
        MoveThrough(application, Processing);

        Assert.Throws<InvalidOperationException>(() => application.MoveTo(SubmittedToEncompass));

        Assert.Equal(Processing, application.Status);
        Assert.Single(application.History);
    }

    [Theory]
    [InlineData(Received)]
    [InlineData(Processing)]
    public void C07_Moving_to_the_current_status_is_not_a_transition(ApplicationStatus status)
    {
        var application = NewApplication();
        if (status != Received)
        {
            MoveThrough(application, status);
        }

        Assert.False(application.CanMoveTo(status));
        Assert.Throws<InvalidOperationException>(() => application.MoveTo(status));
    }

    [Theory]
    [InlineData(Received)]
    [InlineData(NeedsAssignment)]
    [InlineData(IntakeIncomplete)]
    [InlineData(Processing)]
    [InlineData(ReadyForSubmission)]
    [InlineData(SubmissionFailed)]
    public void C08_Cancellation_is_only_possible_during_review(ApplicationStatus status)
    {
        var application = NewApplication();
        MoveThrough(application, PathTo(status));

        Assert.False(application.CanMoveTo(Cancelled));
        Assert.Throws<InvalidOperationException>(() => application.MoveTo(Cancelled));
    }

    [Fact]
    public void C09_Terminal_statuses_allow_nothing()
    {
        var submitted = NewApplication();
        MoveThrough(submitted, PathTo(SubmittedToEncompass));
        var cancelled = NewApplication();
        MoveThrough(cancelled, Processing, ReviewRequired, Cancelled);

        foreach (var status in Enum.GetValues<ApplicationStatus>())
        {
            Assert.False(submitted.CanMoveTo(status));
            Assert.False(cancelled.CanMoveTo(status));
        }
    }

    [Fact]
    public void C10_Unknown_statuses_are_rejected_as_bad_arguments()
    {
        var application = NewApplication();
        var unknown = (ApplicationStatus)999;

        Assert.False(application.CanMoveTo(unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => application.MoveTo(unknown));
    }

    [Fact]
    public void C11_History_records_each_change_with_its_time()
    {
        var application = NewApplication();

        application.MoveTo(Processing);
        _clock.Advance(TimeSpan.FromMinutes(3));
        application.MoveTo(ReviewRequired);
        _clock.Advance(TimeSpan.FromHours(1));
        application.MoveTo(ReviewInProgress);

        Assert.Equal(
            [
                new StatusChange(Received, Processing, Start),
                new StatusChange(Processing, ReviewRequired, Start.AddMinutes(3)),
                new StatusChange(ReviewRequired, ReviewInProgress, Start.AddMinutes(63)),
            ],
            application.History);
    }

    [Fact]
    public void C12_Callers_cannot_rewrite_history()
    {
        var application = NewApplication();
        application.MoveTo(Processing);

        if (application.History is ICollection<StatusChange> collection)
        {
            Assert.True(collection.IsReadOnly);
        }

        if (application.History is IList<StatusChange> list)
        {
            Assert.ThrowsAny<NotSupportedException>(() => list[0] = new StatusChange(Received, Cancelled, Start));
        }

        Assert.Equal(new StatusChange(Received, Processing, Start), Assert.Single(application.History));
    }

    [Fact]
    public void C13_Submission_can_be_retried_three_times()
    {
        var application = NewApplication();
        MoveThrough(application, PathTo(SubmissionFailed));

        for (var retry = 1; retry <= 3; retry++)
        {
            Assert.True(application.CanMoveTo(ReadyForSubmission), $"Retry {retry} should be allowed.");
            MoveThrough(application, ReadyForSubmission, SubmissionFailed);
        }

        Assert.False(application.CanMoveTo(ReadyForSubmission));
        Assert.Throws<InvalidOperationException>(() => application.MoveTo(ReadyForSubmission));
        Assert.Equal(SubmissionFailed, application.Status);
    }

    [Fact]
    public void C14_The_retry_limit_belongs_to_each_application()
    {
        var first = NewApplication();
        var second = NewApplication();
        MoveThrough(first, PathTo(SubmissionFailed));
        MoveThrough(second, PathTo(SubmissionFailed));

        for (var retry = 1; retry <= 3; retry++)
        {
            MoveThrough(first, ReadyForSubmission, SubmissionFailed);
        }

        Assert.False(first.CanMoveTo(ReadyForSubmission));
        Assert.True(second.CanMoveTo(ReadyForSubmission));
    }

    private LoanApplication NewApplication() => new(Guid.NewGuid(), _clock);

    private static void MoveThrough(LoanApplication application, params ApplicationStatus[] statuses)
    {
        foreach (var status in statuses)
        {
            application.MoveTo(status);
        }
    }

    private static ApplicationStatus[] PathTo(ApplicationStatus status) => status switch
    {
        Received => [],
        NeedsAssignment => [NeedsAssignment],
        IntakeIncomplete => [IntakeIncomplete],
        Processing => [Processing],
        ReviewRequired => [Processing, ReviewRequired],
        ReviewInProgress => [Processing, ReviewRequired, ReviewInProgress],
        ReadyForSubmission => [Processing, ReviewRequired, ReviewInProgress, ReadyForSubmission],
        SubmittedToEncompass => [Processing, ReviewRequired, ReviewInProgress, ReadyForSubmission, SubmittedToEncompass],
        SubmissionFailed => [Processing, ReviewRequired, ReviewInProgress, ReadyForSubmission, SubmissionFailed],
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}
