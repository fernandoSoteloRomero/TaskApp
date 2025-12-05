using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApp.DTOs.Auth;

public class RegisterDto
{
  [Required, StringLength(50)] public string UserName { get; set; }

  [Required, EmailAddress, StringLength(100)] public string Email { get; set; }

  [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; }
}
