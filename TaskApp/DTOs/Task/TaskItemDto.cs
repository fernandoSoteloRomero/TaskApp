using System;

namespace TaskApp.DTOs.Task;

public class TaskItemDto
{
  public int Id { get; set; }
  public string Title { get; set; }
  public string? Description { get; set; }
  public DateTime? DueDate { get; set; }

  // * Devolvemos el enum como texto
  public string Status { get; set; }
  public string Priority { get; set; }

  // * Relacion con categoria
  public int CategoryId { get; set; }
  public string CategoryName { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}
