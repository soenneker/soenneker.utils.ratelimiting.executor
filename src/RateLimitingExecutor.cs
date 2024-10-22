using Soenneker.Utils.RateLimiting.Executor.Abstract;
using System.Threading.Tasks;
using System.Threading;
using System;
using Nito.AsyncEx;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Utils.RateLimiting.Executor;

/// <inheritdoc cref="IRateLimitingExecutor"/>
public class RateLimitingExecutor : IRateLimitingExecutor
{
    private readonly TimeSpan _executionInterval;
    private readonly AsyncLock _asyncLock = new();
    private readonly Lazy<CancellationTokenSource> _cancellationTokenSource = new(() => new CancellationTokenSource());
    private DateTime _lastExecutionTime = DateTime.MinValue;

    public RateLimitingExecutor(TimeSpan executionInterval)
    {
        _executionInterval = executionInterval;
    }

    public async ValueTask Execute(Func<CancellationToken, ValueTask> valueTask)
    {
        _cancellationTokenSource.Value.Token.ThrowIfCancellationRequested();

        using (await _asyncLock.LockAsync().ConfigureAwait(false))
        {
            await WaitForNextExecutionAsync().NoSync();

            _cancellationTokenSource.Value.Token.ThrowIfCancellationRequested();

            await valueTask(_cancellationTokenSource.Value.Token).NoSync();
            _lastExecutionTime = DateTime.UtcNow;
        }
    }

    public async ValueTask ExecuteTask(Func<CancellationToken, Task> task)
    {
        _cancellationTokenSource.Value.Token.ThrowIfCancellationRequested();

        using (await _asyncLock.LockAsync().ConfigureAwait(false))
        {
            await WaitForNextExecutionAsync().NoSync();

            _cancellationTokenSource.Value.Token.ThrowIfCancellationRequested();

            await task(_cancellationTokenSource.Value.Token).NoSync();
            _lastExecutionTime = DateTime.UtcNow;
        }
    }

    public void Execute(Action<CancellationToken> action)
    {
        _cancellationTokenSource.Value.Token.ThrowIfCancellationRequested();

        using (_asyncLock.Lock())
        {
            WaitForNextExecution();

            _cancellationTokenSource.Value.Token.ThrowIfCancellationRequested();

            action(_cancellationTokenSource.Value.Token);
            _lastExecutionTime = DateTime.UtcNow;
        }
    }

    private async Task WaitForNextExecutionAsync()
    {
        TimeSpan timeSinceLastExecution = DateTime.UtcNow - _lastExecutionTime;

        if (timeSinceLastExecution < _executionInterval)
        {
            TimeSpan delay = _executionInterval - timeSinceLastExecution;
            await Task.Delay(delay, _cancellationTokenSource.Value.Token).NoSync();
        }
    }

    private void WaitForNextExecution()
    {
        TimeSpan timeSinceLastExecution = DateTime.UtcNow - _lastExecutionTime;

        if (timeSinceLastExecution < _executionInterval)
        {
            TimeSpan delay = _executionInterval - timeSinceLastExecution;
            Task.Delay(delay, _cancellationTokenSource.Value.Token).Wait();
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
