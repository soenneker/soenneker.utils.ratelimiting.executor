using System;
using System.Threading;
using Soenneker.Utils.Delay;

namespace Soenneker.Utils.RateLimiting.Executor;

public partial class RateLimitingExecutor
{
    private T ExecuteInternal<T>(Func<CancellationToken, T> task, CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Value.Token, cancellationToken);
        linkedCts.Token.ThrowIfCancellationRequested();

        using (_asyncLock.Lock(linkedCts.Token))
        {
            WaitForNextExecutionSync(linkedCts.Token);
            linkedCts.Token.ThrowIfCancellationRequested();

            T result = task(linkedCts.Token);
            _lastExecutionTime = DateTime.UtcNow;
            return result;
        }
    }

    public T Execute<T>(Func<CancellationToken, T> action, CancellationToken cancellationToken = default) =>
        ExecuteInternal(action, cancellationToken);

    public T Execute<T, TArg>(Func<CancellationToken, TArg, T> action, TArg argument, CancellationToken cancellationToken = default) =>
        ExecuteInternal(token => action(token, argument), cancellationToken);

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
            DelayUtil.Delay(delay, null, cancellationToken).Wait(cancellationToken);
        }
    }
}