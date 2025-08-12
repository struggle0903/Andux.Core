namespace Andux.Admin.Application.Account.Request
{
    /// <summary>
    /// 登录请求参数
    /// </summary>
    public class LoginRequest
    {
        public string Account { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
