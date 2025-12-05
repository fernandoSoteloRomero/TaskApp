using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApp.DTOs.Auth;

public class RefreshTokenRequestDto
{
  [Required] public string RefreshToken { get; set; }
}
