using Microsoft.EntityFrameworkCore;
using SonicPoints.Data;
using SonicPoints.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly AppDbContext _context;

        public LeaderboardRepository(AppDbContext context)
        {
            _context = context;
        }

        // Fetch leaderboard for the project with user points and task completion count
        public async Task<IEnumerable<Leaderboard>> GetLeaderboardByProjectAsync(int projectId)
        {
            var leaderboard = await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Select(pu => new Leaderboard
                {
                    UserId = pu.UserId,
                    User = pu.User,  // Assuming that the User navigation property is included in the ProjectUsers table
                    PointsEarned = pu.RewardPoints, // Points the user has earned in the project
                    TaskCompletionCount = _context.Tasks.Count(t => t.UserId == pu.UserId && t.ProjectId == projectId),  // Count tasks for the user in this project
                    RedeemedPoints = _context.RedeemHistory
                        .Where(r => r.UserId == pu.UserId && r.ProjectId == projectId) // Filter redeem history for this user and project
                        .Sum(r => r.PointsUsed) // Sum the points the user has redeemed
                })
                .OrderByDescending(l => l.PointsEarned) // Sort by points earned, descending
                .ToListAsync();

            return leaderboard; // Return IEnumerable, not List
        }

        // Fetch total tasks in project for project progress calculation
        public async Task<int> GetTotalTasksInProjectAsync(int projectId)
        {
            return await _context.Tasks.CountAsync(t => t.ProjectId == projectId); // Get the total count of tasks in the project
        }
    }
}
