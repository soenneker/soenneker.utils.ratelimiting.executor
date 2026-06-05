using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Utils.RateLimiting.Executor;

/// <summary>
/// Represents the rate limiting executor.
/// </summary>
public sealed partial class RateLimitingExecutor
{
    private async Task<T> ExecuteTaskInternal<T>(Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Value.Token, cancellationToken);
        linkedCts.Token.ThrowIfCancellationRequested();

        using (await _asyncLock.Lock(linkedCts.Token).NoSync())
        {
            await WaitForNextExecution(linkedCts.Token).NoSync();
            linkedCts.Token.ThrowIfCancellationRequested();

            T result = await task(linkedCts.Token).NoSync();
            _lastExecutionTime = DateTime.UtcNow;
            return result;
        }
    }

    /// <summary>
    /// Executes the execute task operation.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteTask(Func<CancellationToken, Task> task, CancellationToken cancellationToken = default) =>
        await ExecuteTaskInternal(async token =>
        {
            await task(token);
            return 0;
        }, cancellationToken).NoSync();

    /// <summary>
    /// Executes the execute task operation.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    public Task<T> ExecuteTask<T>(Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken = default)
    {
        return ExecuteTaskInternal(task, cancellationToken);
    }

    /// <summary>
    /// Executes the execute task operation.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <typeparam name="TArg">The TArg type.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    public Task<T> ExecuteTask<T, TArg>(Func<CancellationToken, TArg, Task<T>> task, TArg argument, CancellationToken cancellationToken = default)
    {
        return ExecuteTaskInternal(token => task(token, argument), cancellationToken);
    }
}