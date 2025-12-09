using System;
using Microsoft.EntityFrameworkCore;
using TaskApp.Data;
using TaskApp.Models;
using TaskApp.Repositories.GenericRepositories;

namespace TaskApp.Repositories.CategoryRepositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
  public CategoryRepository(ApplicationDbContext db) : base(db)
  {
  }
}
