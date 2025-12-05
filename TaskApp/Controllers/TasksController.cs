using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskApp.Data;
using TaskApp.DTOs.Task;
using TaskApp.Models;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(ApplicationDbContext dbContext) : ControllerBase
    {
        private string GetUserId() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                      ?? throw new UnauthorizedAccessException("Claim 'sub' no presente en el token.");

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAll()
        {
            var userId = GetUserId();

            var list = await dbContext.TaskItems
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .OrderBy(t => t.DueDate)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET api/tasks/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItemDto>> GetById(int id)
        {
            var userId = GetUserId();

            var t = await dbContext.TaskItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.Id == id && x.UserId == userId)
                .Select(x => new TaskItemDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    DueDate = x.DueDate,
                    Status = x.Status.ToString(),
                    Priority = x.Priority.ToString(),
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (t == null) return NotFound();
            return Ok(t);
        }

        // POST api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> Create(
            [FromBody] TaskItemCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            // Verificar que la categoría existe
            var cat = await dbContext.Categories.FindAsync(dto.CategoryId);
            if (cat == null)
                return BadRequest(new { error = "CategoryId inválido" });

            var entity = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = Status.Pending,
                Priority = Enum.Parse<Priority>(dto.Priority, true),
                UserId = userId,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.TaskItems.Add(entity);
            await dbContext.SaveChangesAsync();

            // Mapear a DTO para la respuesta
            var result = new TaskItemDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                DueDate = entity.DueDate,
                Status = entity.Status.ToString(),
                Priority = entity.Priority.ToString(),
                CategoryId = entity.CategoryId,
                CategoryName = cat.Name,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById),
                new { id = result.Id }, result);
        }

        // PUT api/tasks/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] TaskItemUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var entity = await dbContext.TaskItems.FindAsync(id);
            if (entity == null || entity.UserId != userId)
                return NotFound();

            // Verificar categoría
            var cat = await dbContext.Categories.FindAsync(dto.CategoryId);
            if (cat == null)
                return BadRequest(new { error = "CategoryId inválido" });

            // Actualizar campos
            entity.Title = dto.Title ?? entity.Title;
            entity.Description = dto.Description ?? entity.Description;
            entity.DueDate = dto.DueDate ?? entity.DueDate;
            entity.Status = dto.Status != null
                ? Enum.Parse<Status>(dto.Status, true)
                : entity.Status;
            entity.Priority = dto.Priority != null
                ? Enum.Parse<Priority>(dto.Priority, true)
                : entity.Priority;
            entity.CategoryId = dto.CategoryId ?? entity.CategoryId;
            entity.UpdatedAt = DateTime.UtcNow;

            dbContext.TaskItems.Update(entity);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/tasks/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var entity = await dbContext.TaskItems.FindAsync(id);
            if (entity == null || entity.UserId != userId)
                return NotFound();

            dbContext.TaskItems.Remove(entity);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
