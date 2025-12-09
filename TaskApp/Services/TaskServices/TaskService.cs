using System;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using TaskApp.Data;
using TaskApp.DTOs.Common;
using TaskApp.DTOs.Task;
using TaskApp.Models;
using TaskApp.Repositories.TaskRepositories;

namespace TaskApp.Services.TaskServices;

public class TaskService(ITaskRepository taskRepository, IMapper mapper, ApplicationDbContext dbContext) : ITaskService
{
  public async Task<TaskItemDto> CreateAsync(string userId, TaskItemCreateDto taskItemCreateDto)
  {
    var taskItem = mapper.Map<TaskItem>(taskItemCreateDto);
    taskItem.UserId = userId;

    await taskRepository.AddAsync(taskItem);
    await dbContext.SaveChangesAsync();

    var saved = await dbContext.TaskItems
      .Include(c => c.Category)
      .FirstAsync(t => t.Id == taskItem.Id);

    return mapper.Map<TaskItemDto>(saved);
  }

  public async Task<bool> DeleteAsync(string userId, int id)
  {
    var taskItem = await taskRepository.GetByIdAsync(id);

    if (taskItem == null || taskItem.UserId != userId)
    {
      return false;
    }

    taskRepository.Remove(taskItem);
    await dbContext.SaveChangesAsync();

    return true;
  }

  public async Task<PagedResultDto<TaskItemDto>> GetAllAsync(string userId, int pageNumber, int pageSize,
    Status? status,
    Priority? priority, DateTime? DueFrom, DateTime? DueTo)
  {
    var total = await taskRepository.CountAsync(userId, status, priority, DueFrom, DueTo);

    var taskItems = await taskRepository.GetPagedAsync(userId, status, priority, DueFrom, DueTo, pageNumber, pageSize);

    var taskItemDtos = mapper.Map<List<TaskItemDto>>(taskItems);

    return new PagedResultDto<TaskItemDto>
    {
      Items = taskItemDtos,
      TotalCount = total,
      PageNumber = pageNumber,
      PageSize = pageSize,
      TotalPages = (int)Math.Ceiling((double)total / pageSize)
    };
  }

  public async Task<TaskItemDto?> GetByIdAsync(string userId, int id)
  {
    var taskItem = await taskRepository.GetByIdAsync(id);

    if (taskItem == null || taskItem.UserId != userId)
    {
      return null;
    }

    var saved = await dbContext.TaskItems
      .Include(c => c.Category)
      .AsNoTracking()
      .FirstAsync(t => t.Id == taskItem.Id);

    return mapper.Map<TaskItemDto>(saved);
  }

  public async Task<bool> UpdateAsync(string userId, int id, TaskItemUpdateDto taskItemUpdateDto)
  {
    var taskItem = await taskRepository.GetByIdAsync(id);

    if (taskItem == null || taskItem.UserId != userId)
    {
      return false;
    }

    mapper.Map(taskItemUpdateDto, taskItem);

    taskRepository.Update(taskItem);
    await dbContext.SaveChangesAsync();

    return true;
  }
}
