using AwesomeAssertions;
using Soenneker.Tests.FixturedUnit;
using System.Threading.Tasks;
using System;
using Xunit;
using System.Threading;
using Soenneker.Utils.Delay;

namespace Soenneker.Utils.RateLimiting.Executor.Tests;

[Collection("Collection")]
public class RateLimitingExecutorTests : FixturedUnitTest
{
    public RateLimitingExecutorTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    private async Task Method1()
    {
        await Task.CompletedTask;
    }

    private async ValueTask Method2()
    {
        await Task.CompletedTask;
    }

    private async Task<string> Method3()
    {
        return await Task.FromResult("test");
    }

    private async Task Method4(string arg)
    {
        await Task.CompletedTask;
    }

    private async ValueTask<string> Method5(string value)
    {
        return await Task.FromResult(value);
    }

    private async Task<string> Method6(int value)
    {
        return await Task.FromResult("test");
    }

    private async Task<string> Method7(int value, int value2)
    {
        return await Task.FromResult("test");
    }

    private string Method8(CancellationToken cancellationToken)
    {
        return "test";
    }

    private string Method9(string value)
    {
        return value;
    }

    [Fact]
    public async Task Method1Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        await executor.ExecuteTask(_ => Method1(), CancellationToken);
    }

    [Fact]
    public async Task Method2Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        await executor.Execute(_ => Method2(), CancellationToken);
    }

    [Fact]
    public async Task Method3Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        string result = await executor.ExecuteTask(_ => Method3(), CancellationToken);
    }

    [Fact]
    public async Task Method4Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        await executor.ExecuteTask(_ => Method4(""), CancellationToken);
    }

    [Fact]
    public async Task Method5Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        var result = await executor.Execute(_ => Method5(""), CancellationToken);
    }

    [Fact]
    public async Task Method7Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        await executor.ExecuteTask(_ => Method7(4, 3), CancellationToken);
    }

    [Fact]
    public void Method8Test()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        var result = executor.Execute(Method8, CancellationToken);
    }

    [Fact]
    public async Task Execute_ShouldRunTaskWithoutDelay_WhenFirstExecution()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);
        var taskExecuted = false;

        await executor.Execute(async token =>
        {
            taskExecuted = true;
            await Task.CompletedTask;
        }, CancellationToken);

        taskExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_ShouldRespectExecutionInterval_BetweenTasks()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);
        var taskExecuted = false;

        await executor.Execute(async token =>
        {
            taskExecuted = true;
            await Task.CompletedTask;
        }, CancellationToken);

        taskExecuted.Should().BeTrue();

        taskExecuted = false;

        DateTime startTime = DateTime.UtcNow;
        await executor.Execute(async token =>
        {
            taskExecuted = true;
            await Task.CompletedTask;
        }, CancellationToken);

        DateTime endTime = DateTime.UtcNow;

        taskExecuted.Should().BeTrue();

        TimeSpan tolerance = TimeSpan.FromMilliseconds(50);
        (endTime - startTime).Should().BeGreaterThanOrEqualTo(executionInterval - tolerance);
    }

    [Fact]
    public async Task Execute_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        await executor.Execute(async token => { await DelayUtil.Delay(100, null, token); }, CancellationToken);

        await executor.DisposeAsync();

        await FluentActions.Awaiting(async () => await executor.Execute(async token => { await Task.CompletedTask; }))
                           .Should()
                           .ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public void Execute_Action_ShouldRunWithoutDelay_WhenFirstExecution()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);
        var actionExecuted = false;

        executor.Execute(token => { actionExecuted = true; }, CancellationToken);

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ShouldCancelPendingTasks()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        ValueTask executionTask = executor.Execute(async token => { await DelayUtil.Delay(1000, null, token); }, CancellationToken);

        await executor.DisposeAsync();

        await FluentActions.Awaiting(async () => await executionTask).Should().ThrowAsync<OperationCanceledException>();
    }
}