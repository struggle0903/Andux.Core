using Andux.Core.Helper.Config;
using Andux.Core.Helper.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.Helper.Extensions
{
    /// <summary>
    /// 服务使用扩展
    /// </summary>
    public static class HelperServiceCollectionExtensions
    {
        /// <summary>
        /// 使用 Andux.Core.Helper.Http
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseAnduxHelper(this IServiceCollection services)
        {
            // 添加HttpClientFactory服务
            services.AddHttpClient();

            // 注册IHttpHelper为单例
            services.AddSingleton<IHttpHelper>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                return HttpHelper.Create(httpClientFactory, "https://api.example.com"); // 设置基础地址
            });

            // 注册配置帮助类
            services.AddSingleton<ConfigHelper>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new ConfigHelper(configuration);
            });

            return services;
        }

    }
}
