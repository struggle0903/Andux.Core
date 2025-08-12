using Andux.Admin.Application.Account.Request;
using Andux.Admin.Application.Account.Response;

namespace Andux.Admin.Application.Account
{
    /// <summary>
    /// 账户相关接口
    /// </summary>
    public interface IAccountService
    {
        Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, string deviceInfo);

        Task<ApiResult<bool>> LogoutAsync(string sessionId);
    }
}
