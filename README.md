[![](https://img.shields.io/nuget/v/soenneker.utils.ratelimiting.executor.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.ratelimiting.executor/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.ratelimiting.executor/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.ratelimiting.executor/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.ratelimiting.executor.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.ratelimiting.executor/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.RateLimiting.Executor
### A thread-safe utility designed to manage the rate at which tasks are executed, ensuring that they are not run more frequently than a specified interval. 

This can be particularly useful when interacting with rate-limited APIs or for throttling the execution of resource-intensive tasks.

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