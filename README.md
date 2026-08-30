[![](https://img.shields.io/nuget/v/soenneker.utils.ratelimiting.executor.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.ratelimiting.executor/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.ratelimiting.executor/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.ratelimiting.executor/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.ratelimiting.executor.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.ratelimiting.executor/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.ratelimiting.executor/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.ratelimiting.executor/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.RateLimiting.Executor

Serializes delegate execution and enforces a minimum quiet interval after each completed attempt.

## Installation

```bash
dotnet add package Soenneker.Utils.RateLimiting.Executor
```

## Usage

```csharp
using Soenneker.Utils.RateLimiting.Executor;

await using var executor = new RateLimitingExecutor(TimeSpan.FromSeconds(2));

ApiResponse response = await executor.Execute(
    async cancellationToken => await apiClient.Send(cancellationToken),
    cancellationToken);
```

Concurrent callers wait for the same lock. One delegate runs at a time, and the next delegate does
not start until the configured interval has elapsed after the previous delegate completed. A
five-second delegate with a two-second interval therefore makes the next start at least seven
seconds after the previous start. This is not a token bucket, a concurrency limiter, or a
background queue; each caller waits for its own delegate.

The interval is recorded after every delegate that starts, including one that throws or observes
cancellation, so failures cannot bypass throttling. A call canceled while waiting for the lock or
interval never invokes its delegate and does not move the timestamp. Delegate exceptions and
cancellation propagate to that caller.

## Delegate forms

- `Execute(...)` accepts `ValueTask`, result-returning `ValueTask`, and synchronous delegates.
- `ExecuteTask(...)` accepts `Task` and result-returning `Task` delegates.
- Overloads with `TArg` pass state without requiring the caller to capture a closure.

The synchronous overloads block while waiting for both the lock and interval; use an asynchronous
overload in request or UI paths. The executor links each caller token with its own lifetime token
and passes the linked token into the delegate. A delegate that ignores cancellation cannot be
forcibly interrupted.

## Cancellation and disposal

```csharp
executor.CancelExecution();
```

`CancelExecution` is terminal: it cancels the current token and all pending and future calls on
that executor. It is not a pause/reset operation. Dispose or asynchronously dispose the executor
when its lifetime ends; disposal also requests cancellation. Do not start new calls concurrently
with disposal.

For dependency-injection scenarios that need keyed shared executors, see
[Soenneker.Utils.RateLimiting.Factory](https://github.com/soenneker/soenneker.utils.ratelimiting.factory).
