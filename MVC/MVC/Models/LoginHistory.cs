using System;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime LoginTime { get; set; }
    }
}
