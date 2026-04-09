using Andux.Core.EfTenant;
using Andux.Core.EfTenant.Tenant;
using Andux.Core.TenantTesting.Application.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantRepository<User> _repository;
        private readonly ITenantUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="unitOfWork"></param>
        public UserService(
            ITenantProvider tenantProvider,
            IServiceProvider serviceProvider,
            ITenantRepository<User> repository,
            ITenantUnitOfWork unitOfWork)
        {
            _tenantProvider = tenantProvider;

            _serviceProvider = serviceProvider;
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

            try
            {
                var tenantId = _tenantProvider.TryGetTenantId();


                _tenantProvider.SetTenantId(1843473246985566666);


                _unitOfWork.RefreshContext();


                var newTenantId = _tenantProvider.TryGetTenantId();


                await _repository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
            }
            catch(Exception ex)
            {

            }
            finally
            {
                // 清除手动设置的租户ID，恢复从HTTP上下文获取
                _tenantProvider.ClearTenantId();
            }

            return entity.Id;
        }

        /// <summary>
        /// 查询单个用户
        /// </summary>
        public async Task<User?> GetAsync(long id)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var config = db.Set<TenantDbConfig>()
                .AsNoTracking()
                .FirstOrDefault(x => x.TenantId == 1843473246985566666);

            var data = config;


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
