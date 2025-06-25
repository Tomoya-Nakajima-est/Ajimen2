using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ajimen.Models;

namespace Ajimen.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Id);
            if (user == null) return Unauthorized("ユーザーが見つかりません");

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    userId = user.UserName,
                    fullName = user.UserFullName,
                    role = user.UserRole
                });
            }

            return Unauthorized("ログイン失敗");
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.Select(u => new {
                u.UserName,
                u.Email,
                u.UserRole,
                u.UserFullName
            });

            return Ok(await users.ToListAsync());
        }
    }

    public class LoginDto
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }
}
