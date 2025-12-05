using System;

namespace TaskApp.DTOs.Auth;

public class AuthResponseDto
{
  public string AccessToken { get; set; }
  public DateTime AccessTokenExpiresAt { get; set; }
  public string RefreshToken { get; set; }
  public DateTime RefreshTokenExpiresAt { get; set; }
}
