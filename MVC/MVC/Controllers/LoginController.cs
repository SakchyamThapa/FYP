using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using BCrypt.Net;
using System.Linq;
using System.Threading.Tasks;
using MVC.Interface; // Namespace for ILoginRepo

namespace MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;  // DbContext to access Users (if needed)
        private readonly ILoginRepo _loginRepo;  // Injected repository for LoginHistory

        // Constructor for LoginController
        public LoginController(ApplicationDbContext context, ILoginRepo loginRepo)
        {
            _context = context;
            _loginRepo = loginRepo;
        }

        // POST: /Login/Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return Json(new { success = false, message = "Email and password are required" });
            }

            // Find the user by email
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            // Check if the user exists and the password is correct
            if (user == null)
            {
                return Json(new { success = false, message = "Invalid email or password" });
            }

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return Json(new { success = false, message = "Incorrect password" });
            }

            // Log the login attempt using the repository (instead of directly adding to the DbContext)
            var loginHistory = new LoginHistory
            {
                Email = model.Email,
                LoginTime = DateTime.UtcNow
            };

            // Save the login history
            _loginRepo.Insert(loginHistory);
            await _loginRepo.SaveAsync();

            
            HttpContext.Session.SetString("UserEmail", model.Email);

            // Return a success message and redirect to Index page
            return Json(new { success = true, message = "Login successful", redirectTo = Url.Action("Index", "SendMail") });
        }
    }
}
