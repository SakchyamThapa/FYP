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

        // ✅ Get leaderboard for a specific project
        public async Task<IEnumerable<Leaderboard>> GetLeaderboardByProjectAsync(int projectId)
        {
            return await _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Select(pu => new Leaderboard
                {
                    UserId = pu.UserId,
                    User = pu.User,
                    PointsEarned = pu.RewardPoints,
                    TaskCompletionCount = _context.Tasks.Count(t => t.UserId == pu.UserId && t.ProjectId == projectId),
                    RedeemedPoints = _context.RedeemHistory
                        .Where(r => r.UserId == pu.UserId && r.ProjectId == projectId)
                        .Sum(r => r.PointsUsed)
                })
                .OrderByDescending(l => l.PointsEarned)
                .ToListAsync();
        }

        // ✅ Get total number of tasks in a project
        public async Task<int> GetTotalTasksInProjectAsync(int projectId)
        {
            return await _context.Tasks.CountAsync(t => t.ProjectId == projectId);
        }

        // ✅ Get a specific leaderboard entry by task ID and user ID
        public async Task<Leaderboard> GetLeaderboardEntry(int taskId, string userId)
        {
            return await _context.Leaderboards
                .FirstOrDefaultAsync(l => l.TaskId == taskId && l.UserId == userId);
        }

        // ✅ Add a new leaderboard entry
        public async Task AddLeaderboardEntry(Leaderboard leaderboardEntry)
        {
            await _context.Leaderboards.AddAsync(leaderboardEntry);
            await _context.SaveChangesAsync();
        }

        // ✅ Update an existing leaderboard entry
        public async Task UpdateLeaderboardEntry(Leaderboard leaderboardEntry)
        {
            _context.Leaderboards.Update(leaderboardEntry);
            await _context.SaveChangesAsync();
        }
    }
}
