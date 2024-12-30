using System.Collections.Generic;  // Make sure to include this for IEnumerable


namespace MVC.Models
{
    public class AdminPanelViewModel
    {
        public IEnumerable<UserViewModel> users { get; set; }
        public IEnumerable<Project> Projects { get; set; }
        public IEnumerable<RoleModel> Roles { get; set; }
    }
}
