using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApp.Data;
using TaskApp.DTOs.Category;
using TaskApp.Models;
using TaskApp.Services.CategoyServices;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CategoriesController(ICategoryService categoryService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await categoryService.GetAllAsync();
            return Ok(categories);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
        {
            var category = await categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }


        // POST api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        // PUT api/categories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await categoryService.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        // DELETE api/categories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await categoryService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
