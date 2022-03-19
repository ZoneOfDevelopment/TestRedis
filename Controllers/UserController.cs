using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestRedis.Data;

namespace TestRedis.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _userRepository.GetUsers());
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetUserByKey(int key)
        {
            User user = await _userRepository.GetUserByKey(key);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteUser(int key)
        {
            await _userRepository.DeleteUser(key);

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            await _userRepository.AddUser(user);

            return NoContent();
        }
    }
}
