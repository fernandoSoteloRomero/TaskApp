using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskApp.Models;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class RolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        : ControllerBase
    {
        public class AssignRoleDto
        {
            public string UserId { get; set; }
            public string RoleName { get; set; }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var user = await userManager.FindByIdAsync(dto.UserId);
            if (user == null) return NotFound("Usuario no encontrado");

            if (!await roleManager.RoleExistsAsync(dto.RoleName))
            {
                return BadRequest("El rol especificado no existe");
            }

            var res = await userManager.AddToRoleAsync(user, dto.RoleName);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok("Rol asignado exitosamente");
        }


        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            var user = await userManager.FindByIdAsync(dto.UserId);
            if (user == null) return NotFound("Usuario no encontrado");

            if (!await roleManager.RoleExistsAsync(dto.RoleName))
            {
                return BadRequest("El rol especificado no existe");
            }

            var res = await userManager.RemoveFromRoleAsync(user, dto.RoleName);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok("Rol removido exitosamente");
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Usuario no encontrado");

            var roles = await userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }
}
