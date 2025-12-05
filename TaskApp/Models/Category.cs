using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApp.Models;

public class Category
{
  public int Id { get; set; }

  [Required, StringLength(100)]
  public string Name { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  public ICollection<TaskItem> Tasks { get; set; }
}
