using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Asyncs.Locks;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.Delay;
using Soenneker.Utils.RateLimiting.Executor.Abstract;

namespace Soenneker.Utils.RateLimiting.Executor;

/// <inheritdoc cref="IRateLimitingExecutor" />
public sealed partial class RateLimitingExecutor : IRateLimitingExecutor
{
    private readonly TimeSpan _executionInterval;
    private readonly AsyncLock _asyncLock = new();
    private readonly Lazy<CancellationTokenSource> _cancellationTokenSource = new(() => new CancellationTokenSource());
    private DateTimeOffset _lastExecutionTime = DateTimeOffset.MinValue;

    public RateLimitingExecutor(TimeSpan executionInterval)
    {
        _executionInterval = executionInterval;
    }

    private async ValueTask<T> ExecuteValueTaskInternal<T, TArg>(Func<CancellationToken, TArg, ValueTask<T>> valueTask, TArg argument, CancellationToken cancellationToken)
    {
        CancellationToken token = GetExecutionToken(cancellationToken, out CancellationTokenSource? linkedCts);
        using (linkedCts)
        {
            token.ThrowIfCancellationRequested();

            using (await _asyncLock.Lock(token).NoSync())
            {
                await WaitForNextExecution(token).NoSync();
                token.ThrowIfCancellationRequested();

                try
                {
                    return await valueTask(token, argument).NoSync();
                }
                finally
                {
                    _lastExecutionTime = DateTimeOffset.UtcNow;
                }
            }
        }
    }

    private CancellationToken GetExecutionToken(CancellationToken callerToken, out CancellationTokenSource? linkedCts)
    {
        return _cancellationTokenSource.Value.Token.Link(callerToken, out linkedCts);
    }

    public async ValueTask Execute(Func<CancellationToken, ValueTask> valueTask, CancellationToken cancellationToken = default) =>
        await ExecuteValueTaskInternal(static async (token, callback) =>
            {
                await callback(token)
                    .NoSync();
                return 0;
            }, valueTask, cancellationToken)
            .NoSync();

    public ValueTask<T> Execute<T>(Func<CancellationToken, ValueTask<T>> valueTask, CancellationToken cancellationToken = default)
    {
        return ExecuteValueTaskInternal(static (token, callback) => callback(token), valueTask, cancellationToken);
    }

    public async ValueTask Execute<TArg>(Func<CancellationToken, TArg, ValueTask> valueTask, TArg argument, CancellationToken cancellationToken = default) =>
        await ExecuteValueTaskInternal(static async (token, state) =>
            {
                await state.Callback(token, state.Argument)
                    .NoSync();
                return 0;
            }, (Callback: valueTask, Argument: argument), cancellationToken)
            .NoSync();

    public ValueTask<T> Execute<T, TArg>(Func<CancellationToken, TArg, ValueTask<T>> valueTask, TArg argument, CancellationToken cancellationToken = default)
    {
        return ExecuteValueTaskInternal(valueTask, argument, cancellationToken);
    }

    private ValueTask WaitForNextExecution(CancellationToken cancellationToken)
    {
        TimeSpan timeSinceLastExecution = DateTimeOffset.UtcNow - _lastExecutionTime;

        if (timeSinceLastExecution < _executionInterval)
        {
            TimeSpan delay = _executionInterval - timeSinceLastExecution;
            return DelayUtil.Delay(delay, null, cancellationToken);
        }

        return ValueTask.CompletedTask;
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
        if (_cancellationTokenSource.IsValueCreated && !_cancellationTokenSource.Value.IsCancellationRequested)
        {
            await _cancellationTokenSource.Value.CancelAsync()
                                          .NoSync();
        }

        if (_cancellationTokenSource.IsValueCreated)
        {
            _cancellationTokenSource.Value.Dispose();
        }
    }

    public void Dispose()
    {
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
