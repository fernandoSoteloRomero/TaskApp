using System;

namespace TaskApp.DTOs.Category;

public class CategoryDto
{
  public int Id { get; set; }
  public string Name { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

}
