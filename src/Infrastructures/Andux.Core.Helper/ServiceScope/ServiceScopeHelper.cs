using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.Helper.ServiceScope
{
    /// <summary>
    /// 服务作用域帮助类
    /// </summary>
    public static class ServiceScopeHelper
    {
        #region 无返回结果
        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// 同步-无返回值
        /// </summary>
        public static void ExecuteOnly<TService>(IServiceScopeFactory scopeFactory, Action<TService> action)
            where TService : notnull
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            action(service);
        }

        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// 异步-无返回值
        /// </summary>
        public static async Task ExecuteOnlyAsync<TService>(
            IServiceScopeFactory scopeFactory,
            Func<TService, Task> action)
            where TService : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await action(service);
        }

        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// 异步-无返回值
        /// </summary>
        public static async Task ExecuteOnlyAsync<TService1, TService2>(
            IServiceScopeFactory scopeFactory,
            Func<TService1, TService2, Task> action)
            where TService1 : notnull
            where TService2 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service1 = scope.ServiceProvider.GetRequiredService<TService1>();
            var service2 = scope.ServiceProvider.GetRequiredService<TService2>();
            await action(service1, service2);
        }

        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// 异步-无返回值
        /// </summary>
        public static async Task ExecuteOnlyAsync<TService1, TService2, TService3>(
            IServiceScopeFactory scopeFactory,
            Func<TService1, TService2, TService3, Task> action)
            where TService1 : notnull
            where TService2 : notnull
            where TService3 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service1 = scope.ServiceProvider.GetRequiredService<TService1>();
            var service2 = scope.ServiceProvider.GetRequiredService<TService2>();
            var service3 = scope.ServiceProvider.GetRequiredService<TService3>();
            await action(service1, service2, service3);
        }

        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// 异步-无返回值
        /// </summary>
        public static async Task ExecuteOnlyAsync<TService1, TService2, TService3, TService4>(
            IServiceScopeFactory scopeFactory,
            Func<TService1, TService2, TService3, TService4, Task> action)
            where TService1 : notnull
            where TService2 : notnull
            where TService3 : notnull
            where TService4 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service1 = scope.ServiceProvider.GetRequiredService<TService1>();
            var service2 = scope.ServiceProvider.GetRequiredService<TService2>();
            var service3 = scope.ServiceProvider.GetRequiredService<TService3>();
            var service4 = scope.ServiceProvider.GetRequiredService<TService4>();
            await action(service1, service2, service3, service4);
        }

        /// <summary>
        /// 综合方法：在新作用域中执行指定逻辑，支持多个服务类型
        /// 不推荐使用，因为这种方式会损失类型安全、代码提示、DI 注入检查等优势。
        /// </summary>
        /// <param name="scopeFactory"></param>
        /// <param name="serviceTypes"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task ExecuteOnlyAsync(
            IServiceScopeFactory scopeFactory,
            Type[] serviceTypes,
            Func<object[], Task> action)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var provider = scope.ServiceProvider;

            var services = new object[serviceTypes.Length];
            for (int i = 0; i < serviceTypes.Length; i++)
            {
                services[i] = provider.GetRequiredService(serviceTypes[i]);
            }

            await action(services);
        }
        #endregion

        #region 带返回结果
        /// <summary>
        /// 在新作用域中执行指定逻辑，适用于从 Singleton 中安全使用 Scoped 服务。
        /// 带TResult返回
        /// </summary>
        public static TResult ExecuteWithResult<TService, TResult>(IServiceScopeFactory scopeFactory, Func<TService, TResult> func)
            where TService : notnull
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return func(service);
        }

        /// <summary>
        /// 异步：在新作用域中执行异步逻辑（有返回结果）
        /// </summary>
        public static async Task<TResult> ExecuteWithResultAsync<TService, TResult>(
            IServiceScopeFactory scopeFactory,
            Func<TService, Task<TResult>> func)
            where TService : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await func(service);
        }

        /// <summary>
        /// 异步：在新作用域中执行异步逻辑（有返回结果）
        /// </summary>
        public static async Task<TResult> ExecuteWithResultAsync<TService1, TService2, TResult>(
            IServiceScopeFactory scopeFactory,
            Func<TService1, TService2, Task<TResult>> func)
            where TService1 : notnull
            where TService2 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service1 = scope.ServiceProvider.GetRequiredService<TService1>();
            var service2 = scope.ServiceProvider.GetRequiredService<TService2>();
            return await func(service1, service2);
        }

        /// <summary>
        /// 异步：在新作用域中执行异步逻辑（有返回结果）
        /// </summary>
        public static async Task<TResult> ExecuteWithResultAsync<TService1, TService2, TService3, TResult>(
            IServiceScopeFactory scopeFactory,
            Func<TService1, TService2, TService3, Task<TResult>> func)
            where TService1 : notnull
            where TService2 : notnull
            where TService3 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service1 = scope.ServiceProvider.GetRequiredService<TService1>();
            var service2 = scope.ServiceProvider.GetRequiredService<TService2>();
            var service3 = scope.ServiceProvider.GetRequiredService<TService3>();
            return await func(service1, service2, service3);
        }

        /// <summary>
        /// 异步：在新作用域中执行异步逻辑（有返回结果）
        /// </summary>
        public static async Task<TResult> ExecuteWithResultAsync<TService1, TService2, TService3, TService4, TResult>(
            IServiceScopeFactory scopeFactory,
            Func<TService1, TService2, TService3, TService4, Task<TResult>> func)
            where TService1 : notnull
            where TService2 : notnull
            where TService3 : notnull
            where TService4 : notnull
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service1 = scope.ServiceProvider.GetRequiredService<TService1>();
            var service2 = scope.ServiceProvider.GetRequiredService<TService2>();
            var service3 = scope.ServiceProvider.GetRequiredService<TService3>();
            var service4 = scope.ServiceProvider.GetRequiredService<TService4>();
            return await func(service1, service2, service3, service4);
        }

        /// <summary>
        /// 综合方法：在新作用域中执行异步逻辑，支持多个服务类型
        /// 这种方式会损失类型安全、代码提示、DI 注入检查等优势，不建议用于生产环境
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="scopeFactory"></param>
        /// <param name="serviceTypes"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<TResult> ExecuteWithResultAsync<TResult>(
            IServiceScopeFactory scopeFactory,
            Type[] serviceTypes,
            Func<object[], Task<TResult>> func)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var services = serviceTypes.Select(t => scope.ServiceProvider.GetRequiredService(t)).ToArray();
            return await func(services);
        }
        #endregion
    }
}
