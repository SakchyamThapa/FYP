using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SonicPoints.Models;
using SonicPoints.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SonicPoints.Dto;
using Microsoft.AspNetCore.Authorization;

namespace SonicPoints.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // 🔒 Protected route test
        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedRoute()
        {
            var identity = User.Identity;
            var claims = User.Claims.Select(c => new { c.Type, c.Value });

            return Ok(new
            {
                IsAuthenticated = identity?.IsAuthenticated,
                identity?.Name,
                Claims = claims
            });
        }



        // 📝 Register
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

            // ✅ Assign default identity-level role
            await _userManager.AddToRoleAsync(user, "Member");

            return Ok(new { success = true, message = "User registered successfully!" });
        }

        // 🔐 Login + Token
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

        // 🔑 JWT Token Generator
        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var rawKey = Convert.FromBase64String(jwtSettings["Key"]);
            var key = new SymmetricSecurityKey(rawKey);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // ✅ Add role claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
