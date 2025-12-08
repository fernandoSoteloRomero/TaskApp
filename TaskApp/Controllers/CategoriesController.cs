using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApp.Data;
using TaskApp.DTOs.Category;
using TaskApp.Models;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CategoriesController(ApplicationDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var list = await dbContext.Categories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(list);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
        {
            var cat = await dbContext.Categories.AsNoTracking()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (cat == null) return NotFound();

            return Ok(cat);
        }


        // POST api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(
            [FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Category { Name = dto.Name };
            dbContext.Categories.Add(entity);
            await dbContext.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            return CreatedAtAction(nameof(GetCategoryById),
                new { id = result.Id }, result);
        }

        // PUT api/categories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await dbContext.Categories.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;

            dbContext.Categories.Update(entity);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/categories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await dbContext.Categories.FindAsync(id);
            if (entity == null) return NotFound();

            dbContext.Categories.Remove(entity);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
