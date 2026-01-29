using System;

namespace TaskApp.DTOs.Task;

public class TaskItemUpdateDto
{
  public string? Title { get; set; }
  public string? Description { get; set; }
  public DateTime? DueDate { get; set; }

  // * Enviamos el enum como texto
  public string? Status { get; set; }
  
  public string? Priority { get; set; }

  // * Relacion con categoria
  public int? CategoryId { get; set; }
}
