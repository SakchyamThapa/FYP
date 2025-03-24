using SonicPoints.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public interface ILeaderboardRepository
    {
        Task<List<Leaderboard>> GetLeaderboardByProjectAsync(int projectId);
    }
}
