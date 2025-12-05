using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApp.DTOs.Category;

public class CategoryCreateDto
{
  [Required, StringLength(100)]
  public string Name { get; set; }
}
