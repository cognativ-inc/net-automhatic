using System.Collections.Concurrent;
using AutoMHatic.Assessment.Exercise05;

namespace AutoMHatic.Assessment.Tests.Exercise05;

public sealed class EncompassSubmitterTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly FakeEncompassClient _client = new();
    private readonly EncompassSubmitter _submitter;

    public EncompassSubmitterTests()
    {
        _submitter = new EncompassSubmitter(_client);
    }

    [Fact]
    public async Task C01_Invalid_arguments_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new EncompassSubmitter(null!));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _submitter.SubmitAsync(Guid.Empty));
        Assert.Equal(0, _client.Calls);
    }

    [Fact]
    public async Task C02_Submitting_returns_the_Encompass_loan_id()
    {
        var applicationId = Guid.NewGuid();

        var loanId = await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout);

        Assert.Equal(LoanIdFor(applicationId), loanId);
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C03_A_caller_that_already_gave_up_does_not_reach_Encompass()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _submitter.SubmitAsync(Guid.NewGuid(), cancelled.Token));

        Assert.Equal(0, _client.Calls);
    }

    [Fact]
    public async Task C04_A_submitted_application_is_never_sent_again()
    {
        var applicationId = Guid.NewGuid();

        var first = await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout);
        var second = await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout);
        var third = await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout);

        Assert.Equal(first, second);
        Assert.Equal(first, third);
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C05_Transient_failures_are_retried()
    {
        var applicationId = Guid.NewGuid();
        _client.Handler = (id, call, _) => call < 3 ? Transient() : Success(id);

        var loanId = await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout);

        Assert.Equal(LoanIdFor(applicationId), loanId);
        Assert.Equal(3, _client.Calls);
    }

    [Fact]
    public async Task C06_Retrying_stops_after_three_attempts()
    {
        _client.Handler = (_, _, _) => Transient();

        await Assert.ThrowsAsync<EncompassTransientException>(
            () => _submitter.SubmitAsync(Guid.NewGuid()).WaitAsync(Timeout));

        Assert.Equal(3, _client.Calls);
    }

    [Fact]
    public async Task C07_A_rejection_is_not_retried()
    {
        _client.Handler = (_, _, _) => Rejected();

        await Assert.ThrowsAsync<EncompassRejectedException>(
            () => _submitter.SubmitAsync(Guid.NewGuid()).WaitAsync(Timeout));

        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C08_A_failed_submission_can_be_submitted_again()
    {
        var applicationId = Guid.NewGuid();
        _client.Handler = (id, call, _) => call switch
        {
            1 => Rejected(),
            <= 4 => Transient(),
            _ => Success(id),
        };

        await Assert.ThrowsAsync<EncompassRejectedException>(
            () => _submitter.SubmitAsync(applicationId).WaitAsync(Timeout));
        await Assert.ThrowsAsync<EncompassTransientException>(
            () => _submitter.SubmitAsync(applicationId).WaitAsync(Timeout));
        var loanId = await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout);

        Assert.Equal(LoanIdFor(applicationId), loanId);
        Assert.Equal(5, _client.Calls);
    }

    [Fact]
    public async Task C09_Overlapping_submissions_share_one_Encompass_call()
    {
        var applicationId = Guid.NewGuid();
        var gate = _client.HoldCalls();

        var submissions = Enumerable.Range(0, 10).Select(_ => _submitter.SubmitAsync(applicationId)).ToList();
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        submissions.Add(_submitter.SubmitAsync(applicationId));
        gate.SetResult();
        var loanIds = await Task.WhenAll(submissions).WaitAsync(Timeout);

        Assert.All(loanIds, loanId => Assert.Equal(LoanIdFor(applicationId), loanId));
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C10_Submissions_racing_on_many_threads_share_one_Encompass_call()
    {
        const int Rounds = 25;
        const int Threads = 16;
        _client.Handler = async (id, _, _) =>
        {
            await Task.Delay(20);
            return LoanIdFor(id);
        };

        for (var round = 0; round < Rounds; round++)
        {
            var applicationId = Guid.NewGuid();
            using var barrier = new Barrier(Threads);
            var submissions = Enumerable.Range(0, Threads)
                .Select(_ => Task.Factory.StartNew(
                    () =>
                    {
                        barrier.SignalAndWait(Timeout);
                        return _submitter.SubmitAsync(applicationId);
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default).Unwrap())
                .ToArray();

            var loanIds = await Task.WhenAll(submissions).WaitAsync(Timeout);

            Assert.All(loanIds, loanId => Assert.Equal(LoanIdFor(applicationId), loanId));
        }

        Assert.Equal(Rounds, _client.Calls);
    }

    [Fact]
    public async Task C11_Overlapping_submissions_share_a_failure()
    {
        var applicationId = Guid.NewGuid();
        var gate = _client.HoldCalls(then: (_, _, _) => Rejected());

        var first = _submitter.SubmitAsync(applicationId);
        var second = _submitter.SubmitAsync(applicationId);
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        gate.SetResult();

        await Assert.ThrowsAsync<EncompassRejectedException>(() => first.WaitAsync(Timeout));
        await Assert.ThrowsAsync<EncompassRejectedException>(() => second.WaitAsync(Timeout));
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C12_Overlapping_submissions_share_the_retries()
    {
        var applicationId = Guid.NewGuid();
        var gate = _client.HoldCalls(then: (id, call, _) => call == 1 ? Transient() : Success(id));

        var first = _submitter.SubmitAsync(applicationId);
        var second = _submitter.SubmitAsync(applicationId);
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        gate.SetResult();

        Assert.Equal(LoanIdFor(applicationId), await first.WaitAsync(Timeout));
        Assert.Equal(LoanIdFor(applicationId), await second.WaitAsync(Timeout));
        Assert.Equal(2, _client.Calls);
    }

    [Fact]
    public async Task C13_Different_applications_do_not_wait_for_each_other()
    {
        var slow = Guid.NewGuid();
        var fast = Guid.NewGuid();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _client.Handler = async (id, _, _) =>
        {
            if (id == slow)
            {
                await gate.Task;
            }

            return LoanIdFor(id);
        };

        var slowSubmission = _submitter.SubmitAsync(slow);
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);

        Assert.Equal(LoanIdFor(fast), await _submitter.SubmitAsync(fast).WaitAsync(Timeout));
        Assert.False(slowSubmission.IsCompleted);

        gate.SetResult();
        Assert.Equal(LoanIdFor(slow), await slowSubmission.WaitAsync(Timeout));
    }

    [Fact]
    public async Task C14_A_caller_giving_up_does_not_abort_the_submission_for_others()
    {
        var applicationId = Guid.NewGuid();
        var gate = _client.HoldCalls();
        using var impatient = new CancellationTokenSource();

        var impatientSubmission = _submitter.SubmitAsync(applicationId, impatient.Token);
        var patientSubmission = _submitter.SubmitAsync(applicationId);
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);

        await impatient.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => impatientSubmission.WaitAsync(Timeout));

        var lateSubmission = _submitter.SubmitAsync(applicationId);
        gate.SetResult();

        Assert.Equal(LoanIdFor(applicationId), await patientSubmission.WaitAsync(Timeout));
        Assert.Equal(LoanIdFor(applicationId), await lateSubmission.WaitAsync(Timeout));
        Assert.Equal(1, _client.Calls);
        Assert.All(_client.Tokens, token => Assert.False(token.IsCancellationRequested));
    }

    [Fact]
    public async Task C15_The_result_is_kept_even_when_every_caller_gave_up()
    {
        var applicationId = Guid.NewGuid();
        var gate = _client.HoldCalls();
        using var impatient = new CancellationTokenSource();

        var submission = _submitter.SubmitAsync(applicationId, impatient.Token);
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        await impatient.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => submission.WaitAsync(Timeout));
        gate.SetResult();

        Assert.Equal(LoanIdFor(applicationId), await _submitter.SubmitAsync(applicationId).WaitAsync(Timeout));
        Assert.Equal(1, _client.Calls);
    }

    private static string LoanIdFor(Guid applicationId) => $"E-{applicationId:N}";

    private static Task<string> Success(Guid applicationId) => Task.FromResult(LoanIdFor(applicationId));

    private static Task<string> Transient() =>
        Task.FromException<string>(new EncompassTransientException("Encompass timed out."));

    private static Task<string> Rejected() =>
        Task.FromException<string>(new EncompassRejectedException("Borrower SSN is missing."));

    private delegate Task<string> Handler(Guid applicationId, int call, CancellationToken cancellationToken);

    private sealed class FakeEncompassClient : IEncompassClient
    {
        private int _calls;

        public Handler Handler { get; set; } = (id, _, _) => Success(id);

        public int Calls => Volatile.Read(ref _calls);

        public ConcurrentQueue<CancellationToken> Tokens { get; } = new();

        public TaskCompletionSource FirstCallStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource HoldCalls(Handler? then = null)
        {
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var next = then ?? ((id, _, _) => Success(id));
            Handler = async (id, call, cancellationToken) =>
            {
                await gate.Task.WaitAsync(cancellationToken);
                return await next(id, call, cancellationToken);
            };
            return gate;
        }

        public Task<string> CreateLoanFileAsync(Guid applicationId, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _calls);
            Tokens.Enqueue(cancellationToken);
            FirstCallStarted.TrySetResult();
            return Handler(applicationId, call, cancellationToken);
        }
    }
}
