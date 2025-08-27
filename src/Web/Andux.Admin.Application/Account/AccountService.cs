using Andux.Admin.Application.Account.Request;
using Andux.Admin.Application.Account.Response;
using Andux.Admin.Domain.Entitys;
using Andux.Core.EfTrack;

namespace Andux.Admin.Application.Account
{
    /// <summary>
    /// 账户相关服务
    /// </summary>
    public class AccountService : IAccountService
    {

        private readonly IRepository<AnduxUser> _userRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userRepository"></param>
        public AccountService(IRepository<AnduxUser> userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ipAddress"></param>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, string deviceInfo)
        {
            //var user = await _db.AnduxUsers.SingleOrDefaultAsync(u => u.Account == request.Account);
            //if (user == null || !VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            //    throw new UnauthorizedAccessException("账号或密码错误");

            //// 生成 JWT Token，并拿到唯一 SessionId (jti)
            //var tokenResult = _jwtTokenGenerator.GenerateToken(user);

            //// 创建用户会话
            //var session = new UserSession
            //{
            //    UserId = user.Id,
            //    SessionId = tokenResult.Jti,
            //    LoginTime = DateTime.UtcNow,
            //    LastActivityTime = DateTime.UtcNow,
            //    IPAddress = ipAddress,
            //    DeviceInfo = deviceInfo,
            //    IsActive = true,
            //    ExpireTime = tokenResult.ExpireAt
            //};
            //_db.UserSessions.Add(session);

            //// 更新或插入在线用户统计表
            //var onlineUser = await _db.OnlineUsers.SingleOrDefaultAsync(o => o.UserId == user.Id);
            //if (onlineUser == null)
            //{
            //    onlineUser = new OnlineUser
            //    {
            //        UserId = user.Id,
            //        ActiveSessionCount = 1,
            //        LastLoginTime = DateTime.UtcNow,
            //        LastActivityTime = DateTime.UtcNow
            //    };
            //    _db.OnlineUsers.Add(onlineUser);
            //}
            //else
            //{
            //    onlineUser.ActiveSessionCount++;
            //    onlineUser.LastLoginTime = DateTime.UtcNow;
            //    onlineUser.LastActivityTime = DateTime.UtcNow;
            //}

            //await _db.SaveChangesAsync();

            //return new LoginResponse
            //{
            //    Token = tokenResult.Token,
            //    ExpireAt = tokenResult.ExpireAt
            //};

            return ApiResult<LoginResponse>.Success(new LoginResponse());
        }

        public async Task<ApiResult<bool>> LogoutAsync(string sessionId)
        {
            //var session = await _db.UserSessions.SingleOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);
            //if (session == null) return;

            //session.IsActive = false;

            //// 更新在线用户统计
            //var onlineUser = await _db.OnlineUsers.SingleOrDefaultAsync(o => o.UserId == session.UserId);
            //if (onlineUser != null)
            //{
            //    onlineUser.ActiveSessionCount = Math.Max(0, onlineUser.ActiveSessionCount - 1);
            //    onlineUser.LastActivityTime = DateTime.UtcNow;
            //}

            //await _db.SaveChangesAsync();

            return ApiResult<bool>.Success();
        }
    }
}
