namespace SonicPoints.Models
{
    public class ProjectUser
    {
        public int Id { get; set; }  // Primary key for ProjectUser

        // Foreign key to Project
        public int ProjectId { get; set; }  // Ensure this is an int
        public Project Project { get; set; }  // Navigation property to Project

        // Foreign key to User (assuming UserId is string, as it's likely related to Identity)
        public string UserId { get; set; }
        public User User { get; set; }  // Navigation property to User

        public int RewardPoints { get; set; }  // Points earned in this project
        public string Role { get; set; }  // "Member", "Admin"
    }
}
