using System;
using Microsoft.EntityFrameworkCore;
using TaskApp.Data;
using TaskApp.Models;
using TaskApp.Repositories.GenericRepositories;

namespace TaskApp.Repositories.TaskRepositories;

public class TaskRepository
  : GenericRepository<TaskItem>, ITaskRepository
{
  public TaskRepository(ApplicationDbContext db)
    : base(db)
  {
  }

  public async Task<int> CountAsync(string userId, Status? status, Priority? priority, DateTime? DueFrom,
    DateTime? DueTo)
  {
    var q = dbSet.Where(t => t.UserId == userId);
    if (status.HasValue) q = q.Where(t => t.Status == status);
    if (priority.HasValue) q = q.Where(t => t.Priority == priority);
    if (DueFrom.HasValue) q = q.Where(t => t.DueDate >= DueFrom);
    if (DueTo.HasValue) q = q.Where(t => t.DueDate <= DueTo);
    return await q.CountAsync();
  }

  public async Task<IEnumerable<TaskItem>> GetPagedAsync(string userId, Status? status, Priority? priority,
    DateTime? DueFrom,
    DateTime? DueTo, int pageNumber, int pageSize)
  {
    var q = dbSet
      .Include(t => t.Category)
      .Where(t => t.UserId == userId);
    if (status.HasValue) q = q.Where(t => t.Status == status);
    if (priority.HasValue) q = q.Where(t => t.Priority == priority);
    if (DueFrom.HasValue) q = q.Where(t => t.DueDate >= DueFrom);
    if (DueTo.HasValue) q = q.Where(t => t.DueDate <= DueTo);

    return await q
      .OrderBy(t => t.DueDate)
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .AsNoTracking()
      .ToListAsync();
  }
}
