[![](https://img.shields.io/nuget/v/soenneker.utils.ratelimiting.executor.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.ratelimiting.executor/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.ratelimiting.executor/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.ratelimiting.executor/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.ratelimiting.executor.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.ratelimiting.executor/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.RateLimiting.Executor
### A thread-safe utility designed to manage the rate at which tasks are executed, ensuring they are not run more frequently than a specified interval. 

`RateLimitingExecutor` is ideal for interacting with rate-limited APIs or throttling the execution of resource-intensive tasks.

### Sequential Execution

`Tasks`, `ValueTasks`, and `Actions` are executed one at a time. If the defined interval between executions has passed, the task runs immediately; otherwise, it waits until the interval elapses before proceeding.

⚠️ Important Notes:

- This is not a background queue processor. Each method awaits the result of the asynchronous operation before continuing.

- Asynchronous methods will not block the calling thread, but synchronous methods will block execution until it completes.

### Want to use this with dependency injection? 

Check out the singleton factory implementation: [Soenneker.Utils.RateLimiting.Factory](https://github.com/soenneker/soenneker.utils.ratelimiting.factory)

## Installation

```
dotnet add package Soenneker.Utils.RateLimiting.Executor
```

## Example: Executing a Loop of Tasks with Rate Limiting

Below is an example demonstrating how to use the RateLimitingExecutor to execute a series of tasks while maintaining a rate limit.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Soenneker.Utils.RateLimiting.Executor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rateLimitingExecutor = new RateLimitingExecutor(TimeSpan.FromSeconds(2));

        for (int i = 0; i < 5; i++)
        {
            await rateLimitingExecutor.Execute(async ct =>
            {
                Console.WriteLine($"Executing Task {i + 1} at {DateTime.Now:HH:mm:ss}");
                await Task.Delay(100); // Simulate some work
            });
        }
    }
}
```

### Console Output

```csharp
Executing Task 1 at 14:00:00
Executing Task 2 at 14:00:02
Executing Task 3 at 14:00:04
Executing Task 4 at 14:00:06
Executing Task 5 at 14:00:08
```