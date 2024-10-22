using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.RateLimiting.Executor.Abstract;

namespace Soenneker.Utils.RateLimiting.Executor;

///<inheritdoc cref="IRateLimitingExecutor"/>
public partial class RateLimitingExecutor : IRateLimitingExecutor
{
    private readonly TimeSpan _executionInterval;
    private readonly AsyncLock _asyncLock = new();
    private readonly Lazy<CancellationTokenSource> _cancellationTokenSource = new(() => new CancellationTokenSource());
    private DateTime _lastExecutionTime = DateTime.MinValue;

    public RateLimitingExecutor(TimeSpan executionInterval)
    {
        _executionInterval = executionInterval;
    }

    private async ValueTask<T> ExecuteValueTaskInternal<T>(Func<CancellationToken, ValueTask<T>> valueTask, CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Value.Token, cancellationToken);
        linkedCts.Token.ThrowIfCancellationRequested();

        using (await _asyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
        {
            await WaitForNextExecution(linkedCts.Token).NoSync();
            linkedCts.Token.ThrowIfCancellationRequested();

            T result = await valueTask(linkedCts.Token).NoSync();
            _lastExecutionTime = DateTime.UtcNow;
            return result;
        }
    }

    public async ValueTask Execute(Func<CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default) =>
        await ExecuteValueTaskInternal(async token =>
        {
            await valueTask(token).NoSync();
            return 0;
        }, cancellationToken).NoSync();

    public ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> valueTask, CancellationToken cancellationToken = default)
    {
        return ExecuteValueTaskInternal(valueTask, cancellationToken);
    }

    public async ValueTask Execute<TArg>(Func<CancellationToken, TArg, ValueTask> valueTask, TArg argument, CancellationToken cancellationToken = default) =>
        await ExecuteValueTaskInternal(async token =>
        {
            await valueTask(token, argument).NoSync();
            return 0;
        }, cancellationToken).NoSync();

    public ValueTask<T> Execute<T, TArg>(Func<CancellationToken, TArg, ValueTask<T>> valueTask, TArg argument, CancellationToken cancellationToken = default)
    {
        return ExecuteValueTaskInternal(token => valueTask(token, argument), cancellationToken);
    }

    private Task WaitForNextExecution(CancellationToken cancellationToken)
    {
        TimeSpan timeSinceLastExecution = DateTime.UtcNow - _lastExecutionTime;

        if (timeSinceLastExecution < _executionInterval)
        {
            TimeSpan delay = _executionInterval - timeSinceLastExecution;
            return Task.Delay(delay, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public void CancelExecution()
    {
        if (_cancellationTokenSource.IsValueCreated && !_cancellationTokenSource.Value.IsCancellationRequested)
        {
            _cancellationTokenSource.Value.Cancel();
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_cancellationTokenSource.IsValueCreated && !_cancellationTokenSource.Value.IsCancellationRequested)
        {
            await _cancellationTokenSource.Value.CancelAsync().NoSync();
        }

        if (_cancellationTokenSource.IsValueCreated)
        {
            _cancellationTokenSource.Value.Dispose();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_cancellationTokenSource.IsValueCreated && !_cancellationTokenSource.Value.IsCancellationRequested)
        {
            _cancellationTokenSource.Value.Cancel();
        }

        if (_cancellationTokenSource.IsValueCreated)
        {
            _cancellationTokenSource.Value.Dispose();
        }
    }
}