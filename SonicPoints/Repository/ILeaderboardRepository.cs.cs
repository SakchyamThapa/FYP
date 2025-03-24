using SonicPoints.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public interface ILeaderboardRepository
    {
        Task<IEnumerable<Leaderboard>> GetLeaderboardByProjectAsync(int projectId);

       
        Task<int> GetTotalTasksInProjectAsync(int projectId);

    }
}
