using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskApp.Data;
using TaskApp.DTOs.Auth;
using TaskApp.Models;

namespace TaskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext dbContext) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            var user = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "Usuario registrado exitosamente" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized();
            }

            var tokens = await GenerateTokens(user);
            return Ok(tokens);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1) Preparo los parámetros para validar el JWT de refresco
            var rftSection = configuration.GetSection("RefreshTokenSettings");
            var tokenParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rftSection["Key"]!)),
                ValidateIssuer = true,
                ValidIssuer = rftSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = rftSection["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // 2) Uso un handler que no mapea automáticamente los claims ("sub" queda "sub")
            var handler = new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };

            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(
                    dto.RefreshToken,
                    tokenParams,
                    out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwt ||
                    jwt.Header.Alg != SecurityAlgorithms.HmacSha256)
                {
                    return Unauthorized(new { error = "Algoritmo de firma inválido." });
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { error = "Refresh token caducado." });
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Unauthorized(new { error = "Firma del token inválida." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = "Token inválido: " + ex.Message });
            }

            // 3) Extraigo el userId del claim "sub"
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "No hay claim 'sub' en el token." });

            // 4) Busco el RefreshToken en BD
            var oldRft = await dbContext.RefreshTokens
                .SingleOrDefaultAsync(x => x.Token == dto.RefreshToken);
            if (oldRft == null)
                return Unauthorized(new { error = "Refresh token no encontrado en BD." });
            if (!oldRft.IsActive)
                return Unauthorized(new { error = "Refresh token ya no está activo." });

            // 5) Recupero el usuario y genero nuevos tokens
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { error = "Usuario no existe." });

            var newTokens = await GenerateTokens(user);

            // 6) Revoco el token viejo
            oldRft.Revoked = DateTime.UtcNow;
            oldRft.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            oldRft.ReplacedByToken = newTokens.RefreshToken;
            dbContext.RefreshTokens.Update(oldRft);
            await dbContext.SaveChangesAsync();

            return Ok(newTokens);
        }


        private async Task<AuthResponseDto> GenerateTokens(ApplicationUser user)
        {
            var jwtSection = configuration.GetSection("JwtSettings");
            var rftSection = configuration.GetSection("RefreshTokenSettings");

            // 1) ACCESS TOKEN
            var accessKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]));
            var accessCreds = new SigningCredentials(
                accessKey, SecurityAlgorithms.HmacSha256);

            var accessClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var accessExpires = DateTime.UtcNow
                .AddMinutes(int.Parse(jwtSection["AccessTokenExpirationMinutes"]));

            var accessToken = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: accessClaims,
                expires: accessExpires,
                signingCredentials: accessCreds
            );
            var accessTokenString =
                new JwtSecurityTokenHandler().WriteToken(accessToken);

            // 2) REFRESH TOKEN (como JWT)
            var refreshKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(rftSection["Key"]));
            var refreshCreds = new SigningCredentials(
                refreshKey, SecurityAlgorithms.HmacSha256);

            // Puedes incluir sólo el sub + jti, y quizá la fecha
            var refreshClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var refreshExpires = DateTime.UtcNow
                .AddDays(int.Parse(rftSection["RefreshTokenExpirationDays"]));

            var refreshToken = new JwtSecurityToken(
                issuer: rftSection["Issuer"],
                audience: rftSection["Audience"],
                claims: refreshClaims,
                expires: refreshExpires,
                signingCredentials: refreshCreds
            );
            var refreshTokenString =
                new JwtSecurityTokenHandler().WriteToken(refreshToken);

            // 3) Guarda el refresh-token en BD
            var rtEntity = new RefreshToken
            {
                Token = refreshTokenString,
                Expires = refreshExpires,
                Created = DateTime.UtcNow,
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserId = user.Id
            };
            dbContext.RefreshTokens.Add(rtEntity);
            await dbContext.SaveChangesAsync();

            // 4) Devuelve ambos
            return new AuthResponseDto
            {
                AccessToken = accessTokenString,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiresAt = refreshExpires
            };
        }
    }
}
