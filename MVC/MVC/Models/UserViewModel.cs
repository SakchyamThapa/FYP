
namespace MVC.Models
{
  
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int KPIPoints { get; set; }
        public string Roles { get; set; }
        public IEnumerable<Project> Projects { get; set; }
        public IEnumerable<User> users { get; set; }
    }


}
