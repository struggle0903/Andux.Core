namespace Andux.Admin.Application.Account.Response
{
    /// <summary>
    /// 登录响应结果
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
    }
}
