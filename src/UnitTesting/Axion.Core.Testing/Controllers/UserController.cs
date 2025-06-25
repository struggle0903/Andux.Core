using Andux.Core.Common.Result;
using Andux.Core.EfTrack;
using Andux.Core.EfTrack.Repository.Paged;
using Andux.Core.Testing.Controllers.Base;
using Andux.Core.Testing.Entitys;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    public class UserController : ApiBaseController
    {
        private ILogger<UserController> _logger;
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userRepository"></param>
        /// <param name="unitOfWork"></param>
        public UserController(ILogger<UserController> logger,
            IRepository<User> userRepository, 
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<string>>> AddUser()
        {
            var user = new User { Name = "Alice", Age = 28};
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();


            //_logger.LogDebug("【add】用户已添加");
            _logger.LogInformation("【add】用户已添加");
            //_logger.LogError("【add】用户已添加");


            return ApiResponse<string>.Ok(null, "用户已添加");
        }

        [HttpPost("add-batch")]
        public async Task<ActionResult<ApiResponse<string>>> AddUsers()
        {
            var users = new List<User>
        {
            new User { Name = "Bob", Age = 30 },
            new User { Name = "Carol", Age = 25 }
        };
            await _userRepository.AddRangeAsync(users);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.Ok(null, "批量添加成功");
        }

        [HttpGet("getPage")]
        public async Task<ActionResult<ApiResponse<PagedResult<User>>>> GetPagedAsync([FromQuery] BasePageParam param)
        {
            var list = await _userRepository.GetPagedAsync(param);
            return ApiResponse<PagedResult<User>>.Ok(list);
        }

        [HttpGet("get")]
        public async Task<ActionResult<ApiResponse<User?>>> GetByName(string name)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Name == name);
            return ApiResponse<User?>.Ok(user);
        }

        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            return ApiResponse<IEnumerable<User>>.Ok(users);
        }

        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return ApiResponse<string>.Fail("用户不存在");

            user.Age++;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.Ok(null, "用户已更新");
        }

        [HttpDelete("delete")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return ApiResponse<string>.Fail("用户不存在");

            _userRepository.Remove(user);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.Ok(null, "用户已删除");
        }

        [HttpGet("exists")]
        public async Task<ActionResult<ApiResponse<bool>>> Exists(string name)
        {
            var exists = await _userRepository.ExistsAsync(u => u.Name == name);
            return ApiResponse<bool>.Ok(exists);
        }

        [HttpGet("count-adults")]
        public async Task<ActionResult<ApiResponse<int>>> CountAdults()
        {
            var count = await _userRepository.CountAsync(u => u.Age >= 18);
            return ApiResponse<int>.Ok(count);
        }

        [HttpPost("add-transactional")]
        public async Task<ActionResult<ApiResponse<string>>> AddWithTransaction()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _userRepository.AddAsync(new User { Name = "Dave", Age = 31 });
                await _userRepository.AddAsync(new User { Name = "Eve", Age = 29 });
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return ApiResponse<string>.Ok(null, "事务成功提交");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.Fail("发生错误，已回滚事务：" + ex.Message);
            }
        }

    }
}
