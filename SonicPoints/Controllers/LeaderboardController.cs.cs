using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SonicPoints.Dto;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace SonicPoints.Controllers
{
    [Route("api/leaderboard")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardRepository _leaderboardRepository;
        private readonly IMemoryCache _cache;

        public LeaderboardController(ILeaderboardRepository leaderboardRepository, IMemoryCache cache)
        {
            _leaderboardRepository = leaderboardRepository;
            _cache = cache;
        }

        // ✅ Get Leaderboard by Project with Pagination and Caching
        [HttpGet("{projectId}")]
        [Authorize]
        public async Task<IActionResult> GetLeaderboard(int projectId, int pageNumber = 1, int pageSize = 10)
        {
            // Construct cache key based on projectId, pageNumber, and pageSize
            string cacheKey = $"leaderboard_{projectId}_page_{pageNumber}_size_{pageSize}";

            // Check if the leaderboard data is cached
            if (!_cache.TryGetValue(cacheKey, out var cachedLeaderboard))
            {
                try
                {
                    // Fetch the leaderboard for the given project
                    var leaderboard = await _leaderboardRepository.GetLeaderboardByProjectAsync(projectId);

                    if (leaderboard == null || !leaderboard.Any())
                    {
                        return NotFound("No leaderboard data found for this project.");
                    }

                    // Apply pagination (skip and take)
                    var pagedLeaderboard = leaderboard.Skip((pageNumber - 1) * pageSize)
                                                       .Take(pageSize)
                                                       .ToList();

                    // Prepare the result DTO
                    var result = pagedLeaderboard.Select(l => new LeaderboardDto
                    {
                        UserId = l.UserId,
                        UserName = l.User.UserName, // Assuming User has UserName property
                        PointsEarned = l.PointsEarned,
                        TaskCompletionCount = l.TaskCompletionCount
                    }).ToList();

                    // Cache the result
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10)); // Cache for 10 minutes

                    return Ok(result);
                }
                catch (System.Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            // Return cached leaderboard data
            return Ok(cachedLeaderboard);
        }
    }
}
