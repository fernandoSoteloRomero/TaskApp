using System;
using MapsterMapper;
using TaskApp.Data;
using TaskApp.DTOs.Category;
using TaskApp.Models;
using TaskApp.Repositories.CategoryRepositories;

namespace TaskApp.Services.CategoyServices;

public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper, ApplicationDbContext dbContext)
  : ICategoryService
{
  public async Task<CategoryDto> CreateAsync(CategoryCreateDto categoryCreateDto)
  {
    var category = mapper.Map<Category>(categoryCreateDto);
    await categoryRepository.AddAsync(category);
    await dbContext.SaveChangesAsync();
    return mapper.Map<CategoryDto>(category);
  }

  public async Task<bool> DeleteAsync(int id)
  {
    var category = await categoryRepository.GetByIdAsync(id);
    if (category is null)
    {
      return false;
    }

    categoryRepository.Remove(category);
    await dbContext.SaveChangesAsync();
    return true;
  }

  public async Task<IEnumerable<CategoryDto>> GetAllAsync()
  {
    var categories = await categoryRepository.GetAllAsync();
    return mapper.Map<List<CategoryDto>>(categories);
  }

  public async Task<CategoryDto>? GetByIdAsync(int id)
  {
    var category = await categoryRepository.GetByIdAsync(id);
    if (category is null)
    {
      return null;
    }

    return mapper.Map<CategoryDto>(category);
  }

  public async Task<bool> UpdateAsync(int id, CategoryUpdateDto categoryUpdateDto)
  {
    var category = await categoryRepository.GetByIdAsync(id);
    if (category is null)
    {
      return false;
    }

    mapper.Map(categoryUpdateDto, category);
    categoryRepository.Update(category);
    await dbContext.SaveChangesAsync();
    return true;
  }
}