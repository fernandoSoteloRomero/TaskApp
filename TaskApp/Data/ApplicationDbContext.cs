using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskApp.Models;

namespace TaskApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
  public DbSet<TaskItem> TaskItems { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }

}
