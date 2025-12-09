using System;
using TaskApp.DTOs.Category;

namespace TaskApp.Services.CategoyServices;

public interface ICategoryService
{
  Task<IEnumerable<CategoryDto>> GetAllAsync();
  Task<CategoryDto>? GetByIdAsync(int id);
  Task<CategoryDto> CreateAsync(CategoryCreateDto categoryCreateDto);
  Task<bool> UpdateAsync(int id, CategoryUpdateDto categoryUpdateDto);
  Task<bool> DeleteAsync(int id);
}
