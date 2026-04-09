namespace Andux.Core.EfTenant.Tenant
{
    /// <summary>
    /// 租户上下文帮助类
    /// 用于在非HTTP场景（如后台任务、消息队列、定时任务等）中临时设置租户ID
    /// </summary>
    public static class TenantContext
    {
        private static readonly AsyncLocal<long?> _currentTenantId = new AsyncLocal<long?>();
        private static ITenantProvider? _tenantProvider;

        /// <summary>
        /// 初始化租户上下文（在应用启动时调用）
        /// </summary>
        /// <param name="tenantProvider"></param>
        internal static void Initialize(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// 当前租户ID
        /// </summary>
        public static long? CurrentTenantId
        {
            get
            {
                // 优先返回AsyncLocal中的值
                if (_currentTenantId.Value.HasValue)
                    return _currentTenantId.Value.Value;

                // 其次返回Provider中的值
                return _tenantProvider?.TryGetTenantId();
            }
        }

        /// <summary>
        /// 在指定租户上下文中执行代码
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="action">要执行的操作</param>
        public static void RunWithTenant(long tenantId, Action action)
        {
            var previousTenantId = _currentTenantId.Value;
            try
            {
                _currentTenantId.Value = tenantId;
                if (_tenantProvider != null)
                {
                    _tenantProvider.SetTenantId(tenantId);
                }
                action();
            }
            finally
            {
                _currentTenantId.Value = previousTenantId;
                if (_tenantProvider != null)
                {
                    if (previousTenantId.HasValue)
                        _tenantProvider.SetTenantId(previousTenantId.Value);
                    else
                        _tenantProvider.ClearTenantId();
                }
            }
        }

        /// <summary>
        /// 在指定租户上下文中执行异步代码
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="func">要执行的异步操作</param>
        /// <returns></returns>
        public static async Task RunWithTenantAsync(long tenantId, Func<Task> func)
        {
            var previousTenantId = _currentTenantId.Value;
            try
            {
                _currentTenantId.Value = tenantId;
                if (_tenantProvider != null)
                {
                    _tenantProvider.SetTenantId(tenantId);
                }
                await func();
            }
            finally
            {
                _currentTenantId.Value = previousTenantId;
                if (_tenantProvider != null)
                {
                    if (previousTenantId.HasValue)
                        _tenantProvider.SetTenantId(previousTenantId.Value);
                    else
                        _tenantProvider.ClearTenantId();
                }
            }
        }

        /// <summary>
        /// 在指定租户上下文中执行异步代码并返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="tenantId">租户ID</param>
        /// <param name="func">要执行的异步操作</param>
        /// <returns></returns>
        public static async Task<T> RunWithTenantAsync<T>(long tenantId, Func<Task<T>> func)
        {
            var previousTenantId = _currentTenantId.Value;
            try
            {
                _currentTenantId.Value = tenantId;
                if (_tenantProvider != null)
                {
                    _tenantProvider.SetTenantId(tenantId);
                }
                return await func();
            }
            finally
            {
                _currentTenantId.Value = previousTenantId;
                if (_tenantProvider != null)
                {
                    if (previousTenantId.HasValue)
                        _tenantProvider.SetTenantId(previousTenantId.Value);
                    else
                        _tenantProvider.ClearTenantId();
                }
            }
        }

    }
}
