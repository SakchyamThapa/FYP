using System;
using System.ComponentModel.DataAnnotations;



namespace MVC.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Deadline { get; set; }
        public int Progress { get; set; } // Percentage completed
        public string Description { get; set; }

        public string UserId { get; set; } // Foreign key linking to the user
    }
}
