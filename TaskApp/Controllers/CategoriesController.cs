using MapsterMapper;
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
    public class CategoriesController(ApplicationDbContext dbContext, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var entities = await dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var dtos = mapper.Map<List<CategoryDto>>(entities);
            return Ok(dtos);
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
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = mapper.Map<Category>(dto);

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var result = mapper.Map<CategoryDto>(category);
            return CreatedAtAction(nameof(GetCategoryById),
                new { id = result.Id }, result);
        }

        // PUT api/categories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = mapper.Map<Category>(dto);
            category.Id = id;
            dbContext.Categories.Update(category);
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
