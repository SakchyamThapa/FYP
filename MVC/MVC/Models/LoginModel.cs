using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }
    }
}
