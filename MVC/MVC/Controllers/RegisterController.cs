using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using BCrypt.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MVC.Interface;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace MVC.Controllers
{
    public class RegisterController : Controller
    {
        
        public  IRegisterRepo _registerRepo;

        public RegisterController(ApplicationDbContext context, IRegisterRepo registerRepo)
        {

            _registerRepo = registerRepo;
        }

        // POST: /Register/Register
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // 1. Validate the model
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid registration details." });
            }

            // 2. Check if the email already exists
            if (_registerRepo.GetList().Any(u => u.Email == model.Email))
            {
                return Json(new { success = false, message = "Email is already registered" });
            }

            // 3. Check if the password and confirm password match
            if (model.Password != model.ConfirmPassword)
            {
                return Json(new { success = false, message = "Password and Confirm Password do not match" });
            }

            // 4. Validate the password strength (e.g., minimum length, special characters, etc.)
            if (!IsPasswordStrong(model.Password))
            {
                return Json(new { success = false, message = "Password is too weak. It must be at least 8 characters long, include uppercase, lowercase, and a special character." });
            }

            // 5. Hash the password before saving
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // 6. Create the User object to save
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = hashedPassword
            };

            // 7. Add the new user to the database
            _registerRepo.Insert(user);
            
            await _registerRepo.SaveAsync();

            // 8. Return success message
            return Json(new { success = true, message = "Account created successfully" });
        }



        // GET: /Register/GetAll(Read - Retrieve all users)
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _registerRepo.GetList();
            return Json(new { success = true, data = users });
        }

        // GET: /Register/GetById/{id} (Read - Retrieve a user by ID)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _registerRepo.GetList().FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            return Json(new { success = true, data = user });
        }

        // PUT: /Register/Update (Update a user's details)
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] RegisterModel model)
        {
            // 1. Validate the model
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            // 2. Retrieve the user from the database
            var user = _registerRepo.GetList().FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // 3. Update the user fields
            user.FullName = model.FullName;
            user.Email = model.Email;

            // If password is provided, hash it and update
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Password and Confirm Password do not match." });
                }

                if (!IsPasswordStrong(model.Password))
                {
                    return Json(new { success = false, message = "Password is too weak." });
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            // 4. Save changes
            _registerRepo.Update(user);
            await _registerRepo.SaveAsync();

            return Json(new { success = true, message = "User updated successfully." });
        }

        // DELETE: /Register/Delete/{id} (Delete a user)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Retrieve the user from the database
            var user = _registerRepo.GetList().FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // 2. Delete the user
            _registerRepo.Delete(user);
            await _registerRepo.SaveAsync();

            return Json(new { success = true, message = "User deleted successfully." });
        }

       
   

// Helper method to validate password strength
private bool IsPasswordStrong(string password)
        {
            // Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one special character, and one number
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            return regex.IsMatch(password);
        }
    }
}
