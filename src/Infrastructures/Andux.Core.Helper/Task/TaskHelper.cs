namespace Andux.Core.Helper.Tasks
{
    /// <summary>
    /// task 帮助类
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// 在指定时间内运行任务，超时则抛出 TimeoutException。
        /// </summary>
        public static async Task<T> RunWithTimeoutAsync<T>(Func<Task<T>> func, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var task = func();
            var delayTask = Task.Delay(Timeout.Infinite, cts.Token);

            var completedTask = await Task.WhenAny(task, delayTask);
            if (completedTask == task)
            {
                return await task;
            }
            throw new TimeoutException("任务执行超时。");
        }

        /// <summary>
        /// 执行带重试的异步操作。
        /// </summary>
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> action,
            int maxAttempts = 3,
            TimeSpan? delayBetweenAttempts = null)
        {
            delayBetweenAttempts ??= TimeSpan.FromSeconds(1);
            Exception lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(delayBetweenAttempts.Value);
                    }
                }
            }

            throw new Exception($"任务执行失败（已尝试 {maxAttempts} 次）", lastException);
        }

        /// <summary>
        /// Fire and forget（不等待任务），可选异常处理。
        /// </summary>
        public static void FireAndForget(Task task, Action<Exception> onError = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            });
        }

        /// <summary>
        /// 安全运行异步任务，捕获异常，返回是否成功。
        /// </summary>
        public static async Task<bool> RunSafeAsync(Func<Task> action, Action<Exception> onError = null)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// 延迟执行一段时间的任务（支持取消）。
        /// </summary>
        public static async Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            await Task.Delay(delay, cancellationToken);
        }

    }
}
