using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskApp.Models;

public enum Status
{
  Pending,
  InProgress,
  Completed
}

public enum Priority
{
  Low,
  Medium,
  High
}

public class TaskItem
{
  public int Id { get; set; }

  [Required, StringLength(200)]
  public string Title { get; set; }

  public string? Description { get; set; }
  public DateTime? DueDate { get; set; }

  public Status Status { get; set; } = Status.Pending;
  public Priority Priority { get; set; } = Priority.Medium;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  // Relación con el usuario que creó la tarea
  [Required]
  public string UserId { get; set; }

  [ForeignKey(nameof(UserId))]
  public ApplicationUser User { get; set; }

  // Relación con la categoría
  [Required]
  public int CategoryId { get; set; }
  public Category Category { get; set; }
}