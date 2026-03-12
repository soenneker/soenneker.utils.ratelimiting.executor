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
    /// Executes an asynchronous operation with rate limiting.
    /// </summary>
    /// <param name="valueTask">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Execute(Func<CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with rate limiting and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="valueTask">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation and returns a result of type <typeparamref name="T"/>.</returns>
    ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> valueTask, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with rate limiting, with an argument.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="valueTask">The asynchronous operation to execute.</param>
    /// <param name="argument">The argument to pass to the operation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Execute<TArg>(Func<CancellationToken, TArg, ValueTask> valueTask, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with rate limiting, with an argument and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="valueTask">The asynchronous operation to execute.</param>
    /// <param name="argument">The argument to pass to the operation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation and returns a result of type <typeparamref name="T"/>.</returns>
    ValueTask<T> Execute<T, TArg>(Func<CancellationToken, TArg, ValueTask<T>> valueTask, TArg argument, CancellationToken cancellationToken = default);

    Task ExecuteTask(Func<CancellationToken, Task> task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with rate limiting and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="task">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation and returns a result of type <typeparamref name="T"/>.</returns>
    Task<T> ExecuteTask<T>(Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with rate limiting, with an argument and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="task">The asynchronous operation to execute.</param>
    /// <param name="argument">The argument to pass to the operation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation and returns a result of type <typeparamref name="T"/>.</returns>
    Task<T> ExecuteTask<T, TArg>(Func<CancellationToken, TArg, Task<T>> task, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a synchronous operation with rate limiting, with an argument and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="action">The synchronous operation to execute.</param>
    /// <param name="argument">The argument to pass to the operation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The result of type <typeparamref name="T"/>.</returns>
    T Execute<T, TArg>(Func<CancellationToken, TArg, T> action, TArg argument, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a synchronous operation with rate limiting, with an argument and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="action">The synchronous operation to execute.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The result of type <typeparamref name="T"/>.</returns>
    T Execute<T>(Func<CancellationToken, T> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a synchronous operation with rate limiting.
    /// </summary>
    /// <param name="action">The synchronous operation to execute.</param>
    /// <param name="cancellationToken"></param>
    void Execute(Action<CancellationToken> action, CancellationToken cancellationToken = default);

    void CancelExecution();
}
