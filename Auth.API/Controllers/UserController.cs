using Auth.API.DTOs;
using Auth.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // POST /api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = await _userService.Register(
                    dto.UserName, dto.FullName, dto.Email, dto.Password);
                return Ok(new { user.UserId, user.UserName, user.Email });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _userService.Login(dto.Email, dto.Password);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET /api/users/byUsername/{name}
        [HttpGet("byUsername/{name}")]
        public async Task<IActionResult> GetByUsername(string name)
        {
            var user = await _userService.GetUserByUserName(name);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET /api/users/search?q=
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var users = await _userService.SearchUsers(q);
            return Ok(users);
        }

        // PUT /api/users/{id}/profile
        [Authorize]
        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto dto)
        {
            try
            {
                var user = await _userService.UpdateProfile(id, dto.FullName, dto.Bio);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/users/{id}/password
        [Authorize]
        [HttpPut("{id}/password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                await _userService.ChangePassword(id, dto.CurrentPassword, dto.NewPassword);
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/users/{id}/avatar
        [Authorize]
        [HttpPut("{id}/avatar")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
        public async Task<IActionResult> UploadAvatar(int id, IFormFile file)
        {
            try
            {
                var url = await _userService.UploadAvatar(id, file);
                return Ok(new { avatarUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/users/{id}/togglePrivacy
        [Authorize]
        [HttpPut("{id}/togglePrivacy")]
        public async Task<IActionResult> TogglePrivacy(int id)
        {
            try
            {
                await _userService.TogglePrivacy(id);
                return Ok(new { message = "Privacy setting updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/users/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _userService.DeactivateAccount(id);
                return Ok(new { message = "Account deactivated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/users/{id}/counters  ← called by other services
        [HttpPut("{id}/counters")]
        public async Task<IActionResult> UpdateCounters(int id, [FromBody] UpdateCounterDto dto)
        {
            try
            {
                await _userService.UpdateCounters(id, dto.Field, dto.Delta);
                return Ok(new { message = "Counter updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}