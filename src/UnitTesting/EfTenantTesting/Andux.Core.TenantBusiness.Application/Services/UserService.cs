using Andux.Core.EfTenant;
using Andux.Core.TenantTesting.Application.Entitys;

namespace Andux.Core.TenantTesting.Application.Services
{
    public interface IUserService
    {
        Task<long> CreateAsync(string name);

        Task<User?> GetAsync(long id);

        Task<List<User>> GetListAsync(string? keyword = null);

        Task<bool> UpdateAsync(long id, string name);

        Task<bool> DeleteAsync(long id);
    }

    public class UserService: IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            IRepository<User> repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        public async Task<long> CreateAsync(string name)
        {
            var entity = new User
            {
                Name = name
            };

            await _repository.AddAsync(entity);

            await _unitOfWork.SaveChangesAsync();

            return entity.Id;
        }

        /// <summary>
        /// 查询单个用户
        /// </summary>
        public async Task<User?> GetAsync(long id)
        {
            return await _repository.GetAsync(id);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<User>> GetListAsync(string? keyword = null)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await _repository.GetListAsync(x => true);
            }

            return await _repository.GetListAsync(x =>
                x.Name.Contains(keyword));
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        public async Task<bool> UpdateAsync(long id, string name)
        {
            var entity = await _repository.GetAsync(id);

            if (entity == null)
                return false;

            entity.Name = name;

            _repository.Update(entity);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除用户（物理删除）
        /// </summary>
        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _repository.GetAsync(id);

            if (entity == null)
                return false;

            _repository.Remove(entity);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

    }
}
