using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApp.DTOs.Task;

public class TaskItemCreateDto
{
  public string Title { get; set; }
  public string? Description { get; set; }
  public DateTime? DueDate { get; set; }

  // * Enviamos el enum como texto
  public string Status { get; set; }
  public string Priority { get; set; } = "Medium";

  // * Relacion con categoria
  [Required]
  public int CategoryId { get; set; }
}
