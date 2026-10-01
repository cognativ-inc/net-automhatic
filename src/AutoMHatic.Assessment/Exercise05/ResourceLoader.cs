/*
 * Exercise 5 - Concurrent resource loader
 * Difficulty: hard  |  Suggested time: 12 minutes for C01-C08
 *
 * Loading a resource through IResourceClient is slow and expensive. One
 * ResourceLoader instance is shared by many concurrent callers, and it must
 * load each resource as few times as possible.
 *
 * Implement ResourceLoader.GetAsync. It returns the resource for a key.
 *
 * - The constructor throws ArgumentNullException for a null client. GetAsync
 *   throws ArgumentException for a null, empty, or whitespace-only key.
 * - Keys are case-sensitive.
 * - Cache successful results. Never cache a failure: a later call loads again.
 * - Concurrent calls for the same key share one load.
 * - Calls for different keys never wait for each other.
 * - Retry a TransientException, with at most 3 attempts in total. If every
 *   attempt fails, throw the last TransientException. Never retry any other
 *   exception.
 * - A caller's cancellation token only stops that caller from waiting. It never
 *   cancels a load that is shared. A load whose callers all stopped waiting
 *   still finishes, and its result is cached.
 *
 * C01-C08 cover the core requirements. C09-C13 are advanced concurrency and
 * cancellation cases. Passing all of them is not required.
 *
 * Tests: tests/AutoMHatic.Assessment.Tests/Exercise05/ResourceLoaderTests.cs
 */

namespace AutoMHatic.Assessment.Exercise05;

public interface IResourceClient
{
    Task<string> LoadAsync(string key, CancellationToken cancellationToken);
}

public sealed class TransientException : Exception
{
    public TransientException(string message)
        : base(message)
    {
    }
}

public sealed class ResourceLoader
{
    public ResourceLoader(IResourceClient client)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
