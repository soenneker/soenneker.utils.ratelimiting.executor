using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.Task;

namespace Soenneker.Utils.RateLimiting.Executor;

public sealed partial class RateLimitingExecutor
{
    private async Task<T> ExecuteTaskInternal<T>(Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Value.Token, cancellationToken);
        linkedCts.Token.ThrowIfCancellationRequested();

        using (await _asyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
        {
            await WaitForNextExecution(linkedCts.Token).NoSync();
            linkedCts.Token.ThrowIfCancellationRequested();

            T result = await task(linkedCts.Token).NoSync();
            _lastExecutionTime = DateTime.UtcNow;
            return result;
        }
    }

    public async Task ExecuteTask(Func<CancellationToken, Task> task, CancellationToken cancellationToken = default) =>
        await ExecuteTaskInternal(async token =>
        {
            await task(token);
            return 0;
        }, cancellationToken).NoSync();

    public Task<T> ExecuteTask<T>(Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken = default)
    {
        return ExecuteTaskInternal(task, cancellationToken);
    }

    public Task<T> ExecuteTask<T, TArg>(Func<CancellationToken, TArg, Task<T>> task, TArg argument, CancellationToken cancellationToken = default)
    {
        return ExecuteTaskInternal(token => task(token, argument), cancellationToken);
    }
}