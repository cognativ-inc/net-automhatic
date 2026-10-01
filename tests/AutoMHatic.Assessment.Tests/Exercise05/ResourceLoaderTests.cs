using System.Collections.Concurrent;
using AutoMHatic.Assessment.Exercise05;

namespace AutoMHatic.Assessment.Tests.Exercise05;

public sealed class ResourceLoaderTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly FakeResourceClient _client = new();
    private readonly ResourceLoader _loader;

    public ResourceLoaderTests()
    {
        _loader = new ResourceLoader(_client);
    }

    // Core requirements.

    [Fact]
    public async Task C01_Invalid_arguments_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new ResourceLoader(null!));

        foreach (var key in new[] { null, "", "   ", "\t" })
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => _loader.GetAsync(key!));
        }

        Assert.Equal(0, _client.Calls);
    }

    [Fact]
    public async Task C02_Loading_returns_the_resource_from_the_client()
    {
        var value = await _loader.GetAsync("config").WaitAsync(Timeout);

        Assert.Equal(ValueFor("config"), value);
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C03_A_successful_result_is_cached()
    {
        var first = await _loader.GetAsync("config").WaitAsync(Timeout);
        var second = await _loader.GetAsync("config").WaitAsync(Timeout);
        var third = await _loader.GetAsync("config").WaitAsync(Timeout);
        var otherCase = await _loader.GetAsync("CONFIG").WaitAsync(Timeout);

        Assert.Equal(ValueFor("config"), first);
        Assert.Equal(first, second);
        Assert.Equal(first, third);
        Assert.Equal(ValueFor("CONFIG"), otherCase);
        Assert.Equal(2, _client.Calls);
    }

    [Fact]
    public async Task C04_Transient_failures_are_retried()
    {
        _client.Handler = (key, call, _) => call < 3 ? Transient() : Success(key);

        var value = await _loader.GetAsync("config").WaitAsync(Timeout);

        Assert.Equal(ValueFor("config"), value);
        Assert.Equal(3, _client.Calls);
    }

    [Fact]
    public async Task C05_Retrying_stops_after_three_attempts()
    {
        _client.Handler = (_, _, _) => Transient();

        await Assert.ThrowsAsync<TransientException>(() => _loader.GetAsync("config").WaitAsync(Timeout));

        Assert.Equal(3, _client.Calls);
    }

    [Fact]
    public async Task C05_Other_failures_are_not_retried()
    {
        _client.Handler = (_, _, _) => Broken();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _loader.GetAsync("config").WaitAsync(Timeout));

        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C06_A_failure_is_not_cached()
    {
        _client.Handler = (key, call, _) => call switch
        {
            1 => Broken(),
            <= 4 => Transient(),
            _ => Success(key),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _loader.GetAsync("config").WaitAsync(Timeout));
        await Assert.ThrowsAsync<TransientException>(() => _loader.GetAsync("config").WaitAsync(Timeout));
        var value = await _loader.GetAsync("config").WaitAsync(Timeout);

        Assert.Equal(ValueFor("config"), value);
        Assert.Equal(5, _client.Calls);
    }

    [Fact]
    public async Task C07_Concurrent_calls_for_the_same_key_share_one_load()
    {
        var gate = _client.HoldCalls();

        var calls = Enumerable.Range(0, 10).Select(_ => _loader.GetAsync("config")).ToList();
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        calls.Add(_loader.GetAsync("config"));
        gate.SetResult();
        var values = await Task.WhenAll(calls).WaitAsync(Timeout);

        Assert.All(values, value => Assert.Equal(ValueFor("config"), value));
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C08_Calls_for_different_keys_do_not_wait_for_each_other()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _client.Handler = async (key, _, _) =>
        {
            if (key == "slow")
            {
                await gate.Task;
            }

            return ValueFor(key);
        };

        var slow = _loader.GetAsync("slow");
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);

        Assert.Equal(ValueFor("fast"), await _loader.GetAsync("fast").WaitAsync(Timeout));
        Assert.False(slow.IsCompleted);

        gate.SetResult();
        Assert.Equal(ValueFor("slow"), await slow.WaitAsync(Timeout));
    }

    // Advanced concurrency and cancellation cases. Passing these is not required.

    [Fact]
    public async Task C09_Callers_racing_on_many_threads_still_share_one_load()
    {
        const int Rounds = 25;
        const int Threads = 16;
        _client.Handler = async (key, _, _) =>
        {
            await Task.Delay(20);
            return ValueFor(key);
        };

        for (var round = 0; round < Rounds; round++)
        {
            var key = $"resource-{round}";
            using var barrier = new Barrier(Threads);
            var calls = Enumerable.Range(0, Threads)
                .Select(_ => Task.Factory.StartNew(
                    () =>
                    {
                        barrier.SignalAndWait(Timeout);
                        return _loader.GetAsync(key);
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default).Unwrap())
                .ToArray();

            var values = await Task.WhenAll(calls).WaitAsync(Timeout);

            Assert.All(values, value => Assert.Equal(ValueFor(key), value));
        }

        Assert.Equal(Rounds, _client.Calls);
    }

    [Fact]
    public async Task C10_Concurrent_callers_observe_the_same_failure()
    {
        var gate = _client.HoldCalls(then: (_, _, _) => Broken());

        var first = _loader.GetAsync("config");
        var second = _loader.GetAsync("config");
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        gate.SetResult();

        await Assert.ThrowsAsync<InvalidOperationException>(() => first.WaitAsync(Timeout));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.WaitAsync(Timeout));
        Assert.Equal(1, _client.Calls);
    }

    [Fact]
    public async Task C11_Concurrent_callers_share_the_retries()
    {
        var gate = _client.HoldCalls(then: (key, call, _) => call == 1 ? Transient() : Success(key));

        var first = _loader.GetAsync("config");
        var second = _loader.GetAsync("config");
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        gate.SetResult();

        Assert.Equal(ValueFor("config"), await first.WaitAsync(Timeout));
        Assert.Equal(ValueFor("config"), await second.WaitAsync(Timeout));
        Assert.Equal(2, _client.Calls);
    }

    [Fact]
    public async Task C12_A_caller_that_cancels_does_not_cancel_the_load_for_others()
    {
        var gate = _client.HoldCalls();
        using var impatient = new CancellationTokenSource();

        var impatientCall = _loader.GetAsync("config", impatient.Token);
        var patientCall = _loader.GetAsync("config");
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);

        await impatient.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => impatientCall.WaitAsync(Timeout));

        var lateCall = _loader.GetAsync("config");
        gate.SetResult();

        Assert.Equal(ValueFor("config"), await patientCall.WaitAsync(Timeout));
        Assert.Equal(ValueFor("config"), await lateCall.WaitAsync(Timeout));
        Assert.Equal(1, _client.Calls);
        Assert.All(_client.Tokens, token => Assert.False(token.IsCancellationRequested));
    }

    [Fact]
    public async Task C13_A_load_still_populates_the_cache_after_every_caller_cancels()
    {
        var gate = _client.HoldCalls();
        using var impatient = new CancellationTokenSource();

        var call = _loader.GetAsync("config", impatient.Token);
        await _client.FirstCallStarted.Task.WaitAsync(Timeout);
        await impatient.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call.WaitAsync(Timeout));
        gate.SetResult();

        Assert.Equal(ValueFor("config"), await _loader.GetAsync("config").WaitAsync(Timeout));
        Assert.Equal(1, _client.Calls);
    }

    private static string ValueFor(string key) => $"value-of:{key}";

    private static Task<string> Success(string key) => Task.FromResult(ValueFor(key));

    private static Task<string> Transient() =>
        Task.FromException<string>(new TransientException("The request timed out."));

    private static Task<string> Broken() =>
        Task.FromException<string>(new InvalidOperationException("The resource is corrupt."));

    private delegate Task<string> Handler(string key, int call, CancellationToken cancellationToken);

    private sealed class FakeResourceClient : IResourceClient
    {
        private int _calls;

        public Handler Handler { get; set; } = (key, _, _) => Success(key);

        public int Calls => Volatile.Read(ref _calls);

        public ConcurrentQueue<CancellationToken> Tokens { get; } = new();

        public TaskCompletionSource FirstCallStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource HoldCalls(Handler? then = null)
        {
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var next = then ?? ((key, _, _) => Success(key));
            Handler = async (key, call, cancellationToken) =>
            {
                await gate.Task.WaitAsync(cancellationToken);
                return await next(key, call, cancellationToken);
            };
            return gate;
        }

        public Task<string> LoadAsync(string key, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _calls);
            Tokens.Enqueue(cancellationToken);
            FirstCallStarted.TrySetResult();
            return Handler(key, call, cancellationToken);
        }
    }
}
