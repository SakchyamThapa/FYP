namespace SonicPoints.Models
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int RewardPoints { get; set; }  // Points earned in this project
        public string Role { get; set; } // "Member", "Admin"
    }

}
