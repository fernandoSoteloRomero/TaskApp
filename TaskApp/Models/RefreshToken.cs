using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskApp.Models;

public class RefreshToken
{
  [Key] public int Id { get; set; }

  [Required] public string Token { get; set; }

  public DateTime Expires { get; set; }
  public DateTime Created { get; set; } = DateTime.UtcNow;
  public string? CreatedByIp { get; set; }

  public DateTime? Revoked { get; set; }
  public string? RevokedByIp { get; set; }
  public string? ReplacedByToken { get; set; }

  // FK al usuario
  [Required] public string UserId { get; set; }
  [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; }

  // Propiedad de conveniencia
  public bool IsActive => Revoked == null && !IsExpired;
  public bool IsExpired => DateTime.UtcNow >= Expires;
}
