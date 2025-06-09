using Andux.Core.Redis.Helper;
using Andux.Core.Redis.Services;
using Andux.Core.Testing.Entitys;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/redis")]
    public class RedisController : ControllerBase
    {
        private readonly IRedisService _redis;

        public RedisController(IRedisService redis)
        {
            _redis = redis;
        }

        [HttpGet("set")]
        public async Task<IActionResult> Set()
        {
            var result = await _redis.SetAsync("test:key", "hello redis", TimeSpan.FromMinutes(10));

            // 设置缓存
            await RedisHelper.Instance!.SetAsync("key1", "hello world");

            return Ok(new { Success = result });
        }

        [HttpGet("set-list")]
        public async Task<IActionResult> SetList()
        {
            var user = new List<User>();
            user.Add(new Entitys.User()
            {
                Id = 1,
                Name = "Andy",
                Age = 24
            });

            bool setResult = await _redis.SetAsync("user:1", user, TimeSpan.FromMinutes(30));
            return Ok(new { Success = setResult });
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList()
        {
            List<User>? cachedUser = await _redis.GetAsync<List<User>>("user:1");
            return Ok(new { Success = cachedUser });
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var value = await _redis.GetAsync<string>("test:key");
            return Ok(new { Value = value });
        }

        [HttpGet("exists")]
        public async Task<IActionResult> Exists()
        {
            var exists = await _redis.ExistsAsync("test:key");
            return Ok(new { Exists = exists });
        }

        [HttpGet("remove")]
        public async Task<IActionResult> Remove()
        {
            var removed = await _redis.RemoveAsync("test:key");
            return Ok(new { Removed = removed });
        }

        [HttpGet("increment")]
        public async Task<IActionResult> Increment()
        {
            var count = await _redis.IncrementAsync("test:counter");
            return Ok(new { Count = count });
        }

        [HttpGet("decrement")]
        public async Task<IActionResult> Decrement()
        {
            var count = await _redis.DecrementAsync("test:counter");
            return Ok(new { Count = count });
        }

        [HttpGet("expire")]
        public async Task<IActionResult> Expire()
        {
            var success = await _redis.ExpireAsync("test:counter", TimeSpan.FromMinutes(5));
            return Ok(new { Success = success });
        }

        [HttpGet("ttl")]
        public async Task<IActionResult> Ttl()
        {
            var ttl = await _redis.GetTtlAsync("test:counter");
            return Ok(new { TtlSeconds = ttl?.TotalSeconds });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string pattern = "test:*")
        {
            var keys = await _redis.SearchKeysAsync(pattern);
            return Ok(keys);
        }
    }
}
