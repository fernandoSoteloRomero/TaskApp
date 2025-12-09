using System;
using TaskApp.Models;
using TaskApp.Repositories.GenericRepositories;

namespace TaskApp.Repositories.TaskRepositories;

public interface ITaskRepository : IGenericRepository<TaskItem>
{
  Task<int> CountAsync(string userId, Status? status, Priority? priority, DateTime? DueFrom, DateTime? DueTo);

  Task<IEnumerable<TaskItem>> GetPagedAsync(
    string userId, Status? status, Priority? priority, DateTime? DueFrom, DateTime? DueTo, int pageNumber,
    int pageSize);
}
