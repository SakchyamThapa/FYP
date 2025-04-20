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
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Ok(new { success = false, message = "User with this email already exists" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Member");

            return Ok(new { success = true, message = "User registered successfully!", role = "Member" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { success = false, message = "Invalid Email" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { success = false, message = "Invalid Password" });

            var token = await GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful",
                token
            });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            // ✅ Ensure fallback if settings are null
            var issuer = jwtSettings["Issuer"] ?? "https://localhost:7150";
            var audience = jwtSettings["Audience"] ?? "https://localhost:7150";
            var key = jwtSettings["Key"];
            var expirySetting = jwtSettings["ExpiryInMinutes"];
            var expiryMinutes = double.TryParse(expirySetting, out var mins) && mins > 0 ? mins : 60;
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        // Remove issuer and audience from claims - they're set in token descriptor
    };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Create JWT token directly instead of using descriptor
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials
            );

            // Debug check
            Console.WriteLine($"DEBUG - Token created with issuer={token.Issuer}, audience={token.Audiences?.FirstOrDefault()}");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }





        [HttpGet("debug")]
        public IActionResult DebugToken()
        {
            var headers = Request.Headers;
            if (headers.ContainsKey("Authorization"))
            {
                var token = headers["Authorization"].ToString().Replace("Bearer ", "");
                return Ok(new { receivedToken = token });
            }
            return BadRequest("No token found");
        }

        [HttpGet("analyze-token")]
        public IActionResult AnalyzeToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var bearerToken))
                return Unauthorized("No Authorization header");

            var tokenStr = bearerToken.ToString().Replace("Bearer ", "").Trim();

            try
            {
                // Try to read the token without validation
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(tokenStr))
                {
                    var jwtToken = handler.ReadJwtToken(tokenStr);

                    // Extract token details
                    return Ok(new
                    {
                        tokenValid = true,
                        issuer = jwtToken.Issuer,
                        audience = jwtToken.Audiences?.FirstOrDefault(),
                        expiration = jwtToken.ValidTo,
                        claims = jwtToken.Claims.Select(c => new {
                            type = c.Type,
                            value = c.Value
                        })
                    });
                }
                else
                {
                    return BadRequest(new { tokenValid = false, message = "Token format is invalid" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { tokenValid = false, error = ex.Message });
            }
        }

      

    }
}
