using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.RateLimiting.Executor.Abstract;

/// <summary>
/// A thread-safe utility designed to manage the rate at which tasks are executed, ensuring they are not run more frequently than a specified interval.
/// </summary>
public interface IRateLimitingExecutor : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <param name="valueTask">The asynchronous delegate invoked after acquiring a permit.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>An awaitable that completes when the permitted delegate finishes.</returns>
    ValueTask Execute(Func<CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <typeparam name="T">The delegate result type.</typeparam>
    /// <param name="valueTask">The asynchronous delegate invoked after acquiring a permit.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>The delegate's value after a permit is acquired.</returns>
    ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> valueTask, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <typeparam name="TArg">The delegate argument type.</typeparam>
    /// <param name="valueTask">The asynchronous delegate invoked after acquiring a permit.</param>
    /// <param name="argument">The value passed to the delegate.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>An awaitable that completes when the permitted delegate finishes.</returns>
    ValueTask Execute<TArg>(Func<CancellationToken, TArg, ValueTask> valueTask, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <typeparam name="T">The delegate result type.</typeparam>
    /// <typeparam name="TArg">The delegate argument type.</typeparam>
    /// <param name="valueTask">The asynchronous delegate invoked after acquiring a permit.</param>
    /// <param name="argument">The value passed to the delegate.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>The delegate's value after a permit is acquired.</returns>
    ValueTask<T> Execute<T, TArg>(Func<CancellationToken, TArg, ValueTask<T>> valueTask, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied task-returning delegate.
    /// </summary>
    /// <param name="task">The task-returning delegate invoked after acquiring a permit.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>An awaitable that completes when the permitted delegate finishes.</returns>
    Task ExecuteTask(Func<CancellationToken, Task> task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied task-returning delegate.
    /// </summary>
    /// <typeparam name="T">The delegate result type.</typeparam>
    /// <param name="task">The task-returning delegate invoked after acquiring a permit.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>The delegate's value after a permit is acquired.</returns>
    Task<T> ExecuteTask<T>(Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied task-returning delegate.
    /// </summary>
    /// <typeparam name="T">The delegate result type.</typeparam>
    /// <typeparam name="TArg">The delegate argument type.</typeparam>
    /// <param name="task">The task-returning delegate invoked after acquiring a permit.</param>
    /// <param name="argument">The value passed to the delegate.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>The delegate's value after a permit is acquired.</returns>
    Task<T> ExecuteTask<T, TArg>(Func<CancellationToken, TArg, Task<T>> task, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <typeparam name="T">The delegate result type.</typeparam>
    /// <typeparam name="TArg">The delegate argument type.</typeparam>
    /// <param name="action">The synchronous delegate invoked after acquiring a permit.</param>
    /// <param name="argument">The value passed to the delegate.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>An awaitable that completes when the permitted delegate finishes.</returns>
    T Execute<T, TArg>(Func<CancellationToken, TArg, T> action, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <typeparam name="T">The delegate result type.</typeparam>
    /// <param name="action">The synchronous delegate invoked after acquiring a permit.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    /// <returns>An awaitable that completes when the permitted delegate finishes.</returns>
    T Execute<T>(Func<CancellationToken, T> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a rate-limit permit and then invokes the supplied delegate.
    /// </summary>
    /// <param name="action">The synchronous delegate invoked after acquiring a permit.</param>
    /// <param name="cancellationToken">Signals that the operation should stop.</param>
    void Execute(Action<CancellationToken> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently cancels the executor's current, pending, and future operations without disposing its resources.
    /// </summary>
    void CancelExecution();
}
