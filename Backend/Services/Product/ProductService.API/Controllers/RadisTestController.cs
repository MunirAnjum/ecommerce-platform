using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/redis-test")]
    public class RadisTestController : ControllerBase
    {
        private readonly IRedisCacheService _redisCache;

        public RadisTestController(IRedisCacheService redisCache)
        {
            _redisCache = redisCache;   
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var key = "redis-test";

            await _redisCache.SetAsync(key, "Hello from Redis!", TimeSpan.FromMinutes(5));

            var value = await _redisCache.GetAsync<string>(key);

            return Ok(new
            {
                Key = key,
                Value = value
            }); 
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await _redisCache.RemoveAsync("Redis-test");

            return Ok("Redis key deleted");
        }
    }
}
