using System;
using System.Threading;

namespace Soenneker.Utils.RateLimiting.Executor;

/// <summary>
/// Represents the rate limiting executor.
/// </summary>
public sealed partial class RateLimitingExecutor
{
    private T ExecuteInternal<T>(Func<CancellationToken, T> task, CancellationToken cancellationToken)
    {
        CancellationToken token = GetExecutionToken(cancellationToken, out CancellationTokenSource? linkedCts);
        using (linkedCts)
        {
            token.ThrowIfCancellationRequested();

            using (_asyncLock.LockSync(token))
            {
                WaitForNextExecutionSync(token);
                token.ThrowIfCancellationRequested();

                try
                {
                    return task(token);
                }
                finally
                {
                    _lastExecutionTime = DateTimeOffset.UtcNow;
                }
            }
        }
    }

    /// <summary>
    /// Executes the execute operation.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public T Execute<T>(Func<CancellationToken, T> action, CancellationToken cancellationToken = default) =>
        ExecuteInternal(action, cancellationToken);

    /// <summary>
    /// Executes the execute operation.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <typeparam name="TArg">The TArg type.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public T Execute<T, TArg>(Func<CancellationToken, TArg, T> action, TArg argument, CancellationToken cancellationToken = default) =>
        ExecuteInternal(token => action(token, argument), cancellationToken);

    /// <summary>
    /// Executes the execute operation.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public void Execute(Action<CancellationToken> action, CancellationToken cancellationToken = default) =>
        ExecuteInternal(token =>
        {
            action(token);
            return 0;
        }, cancellationToken);

    private void WaitForNextExecutionSync(CancellationToken cancellationToken)
    {
        TimeSpan timeSinceLastExecution = DateTime.UtcNow - _lastExecutionTime;

        if (timeSinceLastExecution < _executionInterval)
        {
            TimeSpan delay = _executionInterval - timeSinceLastExecution;

            if (cancellationToken.WaitHandle.WaitOne(delay))
                cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
