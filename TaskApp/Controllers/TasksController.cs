using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskApp.DTOs.Common;
using TaskApp.DTOs.Task;
using TaskApp.Models;
using TaskApp.Services.TaskServices;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User, Admin")]
    public class TasksController(ITaskService taskService)
        : ControllerBase
    {
        private string GetUserId() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                      ?? throw new UnauthorizedAccessException("Claim 'sub' no presente en el token.");

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<TaskItemDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Status? status = null,
            [FromQuery] Priority? priority = null,
            [FromQuery] DateTime? dueFrom = null,
            [FromQuery] DateTime? dueTo = null)
        {
            var userId = GetUserId();

            var paged = await taskService.GetAllAsync(
                userId,
                pageNumber,
                pageSize,
                status,
                priority,
                dueFrom,
                dueTo);

            return Ok(paged);
        }

        // GET api/tasks/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItemDto>> GetById(int id)
        {
            var userId = GetUserId();

            var taskItemDto = await taskService.GetByIdAsync(userId, id);
            if (taskItemDto == null)
                return NotFound();
            return Ok(taskItemDto);
        }

        // POST api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> Create(
            [FromBody] TaskItemCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var createdTaskItem = await taskService.CreateAsync(userId, dto);

            if (createdTaskItem == null)
                return BadRequest();

            return CreatedAtAction(nameof(GetById), new { id = createdTaskItem.Id }, createdTaskItem);
        }

        // PUT api/tasks/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] TaskItemUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var updatedTaskItem = await taskService.UpdateAsync(userId, id, dto);
            if (!updatedTaskItem)
                return NotFound();

            return NoContent();
        }

        // DELETE api/tasks/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var deleted = await taskService.DeleteAsync(userId, id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
