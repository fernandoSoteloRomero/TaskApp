using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskApp.Data;
using TaskApp.DTOs.Common;
using TaskApp.DTOs.Task;
using TaskApp.Models;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(ApplicationDbContext dbContext, IMapper mapper) : ControllerBase
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

            var query = dbContext.TaskItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.UserId == userId);

            // Filtros
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(x => x.Priority == priority.Value);

            if (dueFrom.HasValue)
                query = query.Where(x => x.DueDate >= dueFrom.Value);

            if (dueTo.HasValue)
                query = query.Where(x => x.DueDate <= dueTo.Value);

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderBy(t => t.DueDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var itemsDto = mapper.Map<List<TaskItemDto>>(items);

            var result = new PagedResultDto<TaskItemDto>
            {
                Items = itemsDto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return Ok(result);
        }

        // GET api/tasks/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItemDto>> GetById(int id)
        {
            var userId = GetUserId();

            var taskItem = await dbContext.TaskItems
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (taskItem == null) return NotFound();

            var dto = mapper.Map<TaskItemDto>(taskItem);

            return Ok(dto);
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

            // Mapear DTO a entidad
            var taskItem = mapper.Map<TaskItem>(dto);
            taskItem.Status = Status.Pending;
            taskItem.UserId = userId;

            dbContext.TaskItems.Add(taskItem);
            await dbContext.SaveChangesAsync();

            // Mapear a DTO para la respuesta
            var taskItemDto = mapper.Map<TaskItemDto>(taskItem);

            return CreatedAtAction(nameof(GetById),
                new { id = taskItemDto.Id }, taskItemDto);
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
            if (dto.CategoryId.HasValue)
            {
                var cat = await dbContext.Categories.FindAsync(dto.CategoryId.Value);
                if (cat == null)
                    return BadRequest(new { error = "CategoryId inválido" });
            }


            // Mapear solo los cambios que el usuario manda
            mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
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
