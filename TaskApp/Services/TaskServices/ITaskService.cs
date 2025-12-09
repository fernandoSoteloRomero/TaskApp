using System;
using TaskApp.DTOs.Common;
using TaskApp.DTOs.Task;
using TaskApp.Models;

namespace TaskApp.Services.TaskServices;

public interface ITaskService
{
  Task<PagedResultDto<TaskItemDto>> GetAllAsync(
    string userId,
    int pageNumber,
    int pageSize,
    Status? status,
    Priority? priority,
    DateTime? DueFrom,
    DateTime? DueTo);

  Task<TaskItemDto?> GetByIdAsync(string userId, int id);

  Task<TaskItemDto> CreateAsync(string userId, TaskItemCreateDto taskItemCreateDto);

  Task<bool> UpdateAsync(string userId, int id, TaskItemUpdateDto taskItemUpdateDto);
  Task<bool> DeleteAsync(string userId, int id);
}
