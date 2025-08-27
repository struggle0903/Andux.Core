using Andux.Admin.Application.Account.Request;
using Andux.Admin.Application.Account.Response;

namespace Andux.Admin.Application.Account
{
    /// <summary>
    /// 账户相关接口
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ipAddress"></param>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, string deviceInfo);

        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<ApiResult<bool>> LogoutAsync(string sessionId);
    }
}
