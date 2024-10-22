using System;
using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.Utils.RateLimiting.Executor.Abstract;

/// <summary>
/// A thread-safe utility designed to manage the rate at which tasks are executed, ensuring that they are not run more frequently than a specified interval.
/// </summary>
public interface IRateLimitingExecutor : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Executes an asynchronous task while respecting the rate limit.
    /// </summary>
    /// <param name="valueTask">A function that takes a <see cref="CancellationToken"/> and returns a <see cref="ValueTask"/> representing the asynchronous operation to execute.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the completion of the execution.</returns>
    ValueTask Execute(Func<CancellationToken, ValueTask> valueTask);

    /// <summary>
    /// Executes an asynchronous task while respecting the rate limit.
    /// </summary>
    /// <param name="task">A function that takes a <see cref="CancellationToken"/> and returns a <see cref="Task"/> representing the asynchronous operation to execute.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the completion of the execution.</returns>
    ValueTask ExecuteTask(Func<CancellationToken, Task> task);

    /// <summary>
    /// Executes a synchronous task while respecting the rate limit.
    /// </summary>
    /// <param name="action">An action that takes a <see cref="CancellationToken"/> representing the operation to execute.</param>
    void Execute(Action<CancellationToken> action);
}
