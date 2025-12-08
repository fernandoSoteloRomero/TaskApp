using TaskApp.Middlewares;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskApp.Data;
using TaskApp.Models;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1) Configuración de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
  opts.UseSqlServer(
    config.GetConnectionString("DefaultConnection")
  ));

// 2) Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
  {
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireDigit = true;
  })
  .AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();

// 3) JWT
var jwt = config.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);
builder.Services.AddAuthentication(options =>
  {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(options =>
  {
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = jwt["Issuer"],
      ValidateAudience = true,
      ValidAudience = jwt["Audience"],
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      RoleClaimType = "role"
    };
  });

// 4) Controllers + Swagger + CORS
builder.Services.AddControllers();
builder.Services.AddCors(o =>
  o.AddPolicy("AllowFrontend", p => p
    .WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new() { Title = "TaskApp API", Version = "v1" });
  c.AddSecurityDefinition("Bearer", new()
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Ingrese 'Bearer {token}'"
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      Array.Empty<string>()
    }
  });
});

var app = builder.Build();

// ----------------------------------------------------------
// A) Auto-migrate (solo en Development, opcional en Prod)
// ----------------------------------------------------------
if (app.Environment.IsDevelopment())
{
  using var migScope = app.Services.CreateScope();
  var db = migScope.ServiceProvider
    .GetRequiredService<ApplicationDbContext>();
  db.Database.Migrate();
}

// ----------------------------------------------------------
// B) Seed de Roles + Asignación de "User" + Admin predeterm.
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
  var roleMgr = scope.ServiceProvider
    .GetRequiredService<RoleManager<IdentityRole>>();
  var userMgr = scope.ServiceProvider
    .GetRequiredService<UserManager<ApplicationUser>>();

  // B.1) Crear roles si no existen
  foreach (var roleName in new[] { "Admin", "User" })
    if (!await roleMgr.RoleExistsAsync(roleName))
      await roleMgr.CreateAsync(new IdentityRole(roleName));

  // B.2) Asignar "User" a todos los usuarios sin roles
  var allUsers = await userMgr.Users.ToListAsync();
  foreach (var u in allUsers)
  {
    var userRoles = await userMgr.GetRolesAsync(u);
    if (userRoles.Count == 0)
      await userMgr.AddToRoleAsync(u, "User");
  }

  // B.3) Sembrar Admin (lee config AdminUser:Email/Password)
  var adminEmail = config["AdminUser:Email"];
  var adminPassword = config["AdminUser:Password"];
  if (!string.IsNullOrWhiteSpace(adminEmail)
      && !string.IsNullOrWhiteSpace(adminPassword))
  {
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
      admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
      var res = await userMgr.CreateAsync(admin, adminPassword);
      if (res.Succeeded)
        await userMgr.AddToRolesAsync(admin, new[] { "Admin", "User" });
    }
    else
    {
      var existing = await userMgr.GetRolesAsync(admin);
      if (!existing.Contains("Admin"))
        await userMgr.AddToRoleAsync(admin, "Admin");
      if (!existing.Contains("User"))
        await userMgr.AddToRoleAsync(admin, "User");
    }
  }
}

// ----------------------------------------------------------
// C) Pipeline normal
// ----------------------------------------------------------
app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskApp API v1");
    c.RoutePrefix = string.Empty;
  });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();