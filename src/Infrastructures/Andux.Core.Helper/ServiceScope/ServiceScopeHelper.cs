using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.Helper.ServiceScope
{
    /// <summary>
    /// 服务作用域帮助类
    /// </summary>
    public static class ServiceScopeHelper
    {
        #region 单个 Scoped 服务
        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// </summary>
        public static void ExecuteInScope<TService>(IServiceScopeFactory scopeFactory, Action<TService> action)
            where TService : notnull
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            action(service);
        }

        /// <summary>
        /// 支持返回结果的版本
        /// </summary>
        public static TResult ExecuteInScope<TService, TResult>(IServiceScopeFactory scopeFactory, Func<TService, TResult> func)
            where TService : notnull
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return func(service);
        }

        /// <summary>
        /// 异步：在新作用域中执行异步逻辑（无返回值）
        /// </summary>
        public static async Task ExecuteInScopeAsync<TService>(
            IServiceScopeFactory scopeFactory,
            Func<TService, Task> action)
            where TService : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await action(service);
        }

        /// <summary>
        /// 异步：在新作用域中执行异步逻辑并返回结果
        /// </summary>
        public static async Task<TResult> ExecuteInScopeAsync<TService, TResult>(
            IServiceScopeFactory scopeFactory,
            Func<TService, Task<TResult>> func)
            where TService : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await func(service);
        }
        #endregion

        #region 同时解析两个 Scoped 服务
        /// <summary>
        /// 同时解析两个 Scoped 服务，适用于在 Singleton 中使用多个 Scoped 服务
        /// </summary>
        public static void ExecuteInScope<T1, T2>(IServiceScopeFactory scopeFactory, Action<T1, T2> action)
            where T1 : notnull
            where T2 : notnull
        {
            using var scope = scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;

            var s1 = sp.GetRequiredService<T1>();
            var s2 = sp.GetRequiredService<T2>();

            action(s1, s2);
        }

        /// <summary>
        /// 支持带返回值的两个服务组合使用
        /// </summary>
        public static TResult ExecuteInScope<T1, T2, TResult>(IServiceScopeFactory scopeFactory, Func<T1, T2, TResult> func)
            where T1 : notnull
            where T2 : notnull
        {
            using var scope = scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;

            var s1 = sp.GetRequiredService<T1>();
            var s2 = sp.GetRequiredService<T2>();

            return func(s1, s2);
        }

        public static async Task ExecuteInScopeAsync<T1, T2>(
            IServiceScopeFactory scopeFactory,
            Func<T1, T2, Task> action)
            where T1 : notnull
            where T2 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sp = scope.ServiceProvider;

            var s1 = sp.GetRequiredService<T1>();
            var s2 = sp.GetRequiredService<T2>();

            await action(s1, s2);
        }
        #endregion

    }
}
