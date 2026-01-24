using CacheStampede.Model;
using CacheStampede.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CacheStampede.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(CachedRepository cachedRepository) : ControllerBase
    {
        private readonly CachedRepository _cachedRepository = cachedRepository;

        [HttpGet("GetUserDetail/{userID:int}")]
        public async Task<IActionResult> GetUser(int userID, CancellationToken ct)

        {
            try
            {
                var user = await _cachedRepository.GetOrLoadAsync<User>($"USER_{userID}", () => GetUserFromDB(userID), TimeSpan.FromMinutes(2), ct);

                if (user is not null)
                {
                    return Ok(user);
                }

                return NotFound($"User {userID} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [NonAction]
        private static async Task<User?> GetUserFromDB(int userID)
        {
            await Task.Delay(100);
            return UserService.GetUser(userID);
        }
    }
}
