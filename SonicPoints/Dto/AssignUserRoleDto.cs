namespace SonicPoints.DTOs
{
    public class AssignUserRoleDto
    {
        public string UserId { get; set; }
        public string AdminId { get; set; }  // AdminId of the current user performing the role assignment
        public string Role { get; set; }  // Role to assign (Admin, Manager, Member)
    }
}
