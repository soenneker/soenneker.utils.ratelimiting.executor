using FluentAssertions;
using Soenneker.Tests.FixturedUnit;
using System.Threading.Tasks;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Utils.RateLimiting.Executor.Tests;

[Collection("Collection")]
public class RateLimitingExecutorTests : FixturedUnitTest
{
    public RateLimitingExecutorTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
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
        });

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
        });

        taskExecuted.Should().BeTrue();

        taskExecuted = false;

        DateTime startTime = DateTime.UtcNow;
        await executor.Execute(async token =>
        {
            taskExecuted = true;
            await Task.CompletedTask;
        });
        DateTime endTime = DateTime.UtcNow;

        taskExecuted.Should().BeTrue();

        TimeSpan tolerance = TimeSpan.FromMilliseconds(50);
        (endTime - startTime).Should().BeGreaterOrEqualTo(executionInterval - tolerance);
    }

    [Fact]
    public async Task Execute_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        await executor.Execute(async token =>
        {
            await Task.Delay(100, token);
        });

        await executor.DisposeAsync();

        await FluentActions.Awaiting(async () =>
            await executor.Execute(async token =>
            {
                await Task.CompletedTask;
            })).Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public void Execute_Action_ShouldRunWithoutDelay_WhenFirstExecution()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);
        var actionExecuted = false;

        executor.Execute(token =>
        {
            actionExecuted = true;
        });

        actionExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ShouldCancelPendingTasks()
    {
        TimeSpan executionInterval = TimeSpan.FromMilliseconds(500);
        var executor = new RateLimitingExecutor(executionInterval);

        ValueTask executionTask = executor.Execute(async token =>
        {
            await Task.Delay(1000, token);
        });

        await executor.DisposeAsync();

        await FluentActions.Awaiting(async () => await executionTask)
            .Should().ThrowAsync<OperationCanceledException>();
    }
}
