using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SonicPoints.Models;
using SonicPoints.DTOs;
using SonicPoints.Dto;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SonicPoints.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedRoute()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new
            {
                message = "✅ You are authorized!",
                userId,
                userName,
                roles
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { success = false, message = "User with this email already exists" });

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { success = false, errors = result.Errors });

            await _userManager.AddToRoleAsync(user, "Member");

            return Ok(new { success = true, message = "User registered successfully!", role = "Member" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { success = false, message = "Invalid email or password" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { success = false, message = "Invalid email or password" });

            var token = await GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful",
                token,
                userId = user.Id,
                username = user.UserName,
                email = user.Email
            });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var issuer = _configuration.GetValue<string>("Jwt:Issuer") ?? "https://localhost:7150";
            var audience = _configuration.GetValue<string>("Jwt:Audience") ?? "https://localhost:7150";
            var key = _configuration.GetValue<string>("Jwt:Key");
            var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryInMinutes");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpGet("analyze-token")]
        public IActionResult AnalyzeToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var bearerToken))
                return Unauthorized(new { success = false, message = "No Authorization header found" });

            var tokenStr = bearerToken.ToString().Replace("Bearer ", "").Trim();
            var jwtSettings = _configuration.GetSection("Jwt");

            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(tokenStr))
                    return BadRequest(new { tokenValid = false, message = "Token format is invalid" });

                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.GetValue<string>("Issuer") ?? "https://localhost:7150",
                    ValidAudience = jwtSettings.GetValue<string>("Audience") ?? "https://localhost:7150",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                ClaimsPrincipal principal = handler.ValidateToken(tokenStr, validationParams, out SecurityToken validatedToken);

                return Ok(new
                {
                    tokenValid = true,
                    issuer = ((JwtSecurityToken)validatedToken).Issuer,
                    audience = ((JwtSecurityToken)validatedToken).Audiences.FirstOrDefault(),
                    expiration = validatedToken.ValidTo,
                    claims = principal.Claims.Select(c => new { type = c.Type, value = c.Value })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { tokenValid = false, error = ex.Message });
            }
        }
    }
}
