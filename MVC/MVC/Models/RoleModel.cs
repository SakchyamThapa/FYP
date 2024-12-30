namespace MVC.Models
{
    public class RoleModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<RoleModel> Roles { get; set; }
    }
}
