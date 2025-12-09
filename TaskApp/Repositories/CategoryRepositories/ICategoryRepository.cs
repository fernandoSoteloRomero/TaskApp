using System;
using TaskApp.Models;
using TaskApp.Repositories.GenericRepositories;

namespace TaskApp.Repositories.CategoryRepositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
}
