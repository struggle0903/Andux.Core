// =======================================
// 作者：andy.hu
// 文件：AuditingInterceptor.cs
// 描述：自动设置审计字段的拦截器，注册为 SaveChanges 前处理逻辑
// =======================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 审计字段拦截器，在保存前统一设置时间戳和用户信息。
    /// 可通过配置项控制是否启用。
    /// </summary>
    public class AuditingInterceptor : SaveChangesInterceptor
    {

        private readonly EntityBehaviorOptions _options;

        /// <summary>
        /// HttpContextAccessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        /// <param name="accessor"></param>
        public AuditingInterceptor(IOptions<EntityBehaviorOptions> options, IHttpContextAccessor accessor)
        {
            _options = options.Value;
            _httpContextAccessor = accessor;
        }

        /// <summary>
        /// 拦截 SaveChangesAsync，在变更实体前自动设置审计字段
        /// </summary>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null || !_options.EnableAuditing)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var entries = context.ChangeTracker.Entries<IAuditedEntity>();
            var dateNow = DateTime.Now;

            // 从上下文中获取真实用户标识
            var currentUser = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == _options.UserClaimsType)?.Value ?? string.Empty;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedTime = dateNow;
                    entry.Entity.CreatedBy = currentUser;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedTime = dateNow;
                    entry.Entity.UpdatedBy = currentUser;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

    }
}
