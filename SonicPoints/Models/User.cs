using Microsoft.AspNetCore.Identity;

namespace SonicPoints.Models
{
    public class User : IdentityUser
    {
        
        public List<ProjectUser> ProjectUsers { get; set; }
        public List<Leaderboard> TaskCompletions { get; set; }
        public List<RedeemHistory> RedeemHistories { get; set; }
    }

}
