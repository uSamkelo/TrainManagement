using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs;
using UserManagementService.Models;
using UserManagementService.Services.Interfaces;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,StationStaff")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("role/{role}")]
        [Authorize(Roles = "Admin,StationStaff")]
        public async Task<IActionResult> GetByRole(string role)
        {
            if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
                return BadRequest(new { error = "Invalid role." });
            return Ok(await _userService.GetByRoleAsync(parsedRole));
        }

        [HttpGet("drivers")]
        [Authorize(Roles = "Admin,StationStaff")]
        public async Task<IActionResult> GetDrivers()
        {
            return Ok(await _userService.GetByRoleAsync(UserRole.Driver));
        }

        [HttpGet("station-staff")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStationStaff()
        {
            return Ok(await _userService.GetByRoleAsync(UserRole.StationStaff));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.UpdateAsync(id, request);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _userService.DeactivateAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deactivated." });
        }
    }
}
