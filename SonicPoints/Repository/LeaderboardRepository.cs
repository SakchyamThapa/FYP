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
        public async Task<List<Leaderboard>> GetLeaderboardByProjectAsync(int projectId)
        {
            return await _context.Tasks
                .Where(t => t.ProjectId == projectId) // Filter tasks by projectId
                .GroupBy(t => t.UserId) // Group tasks by UserId
                .Select(group => new Leaderboard
                {
                    UserId = group.Key, // UserId associated with the tasks
                    User = group.FirstOrDefault().User, // Get User details from the first task in the group
                    PointsEarned = group.Sum(t => t.RewardPoints), // Sum of points for each task completed
                    TaskCompletionCount = group.Count() // Count tasks completed by the user
                })
                .OrderByDescending(l => l.PointsEarned) // Order by points earned in descending order
                .ToListAsync();
        }
    }
}
