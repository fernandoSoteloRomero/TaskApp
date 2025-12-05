using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApp.DTOs.Category;

public class CategoryUpdateDto
{
  [Required, StringLength(100)]
  public string Name { get; set; }
}
