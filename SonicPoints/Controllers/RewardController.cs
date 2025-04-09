using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using SonicPoints.Dto;

namespace SonicPoints.Controllers
{
    [Route("api/rewards")]
    [ApiController]
    [Authorize]
    public class RewardController : ControllerBase
    {
        private readonly IRewardRepository _rewardRepository;
        private readonly ILeaderboardRepository _leaderboardRepository;
        private readonly IProjectRepository _projectRepository;

        public RewardController(IRewardRepository rewardRepository, ILeaderboardRepository leaderboardRepository, IProjectRepository projectRepository)
        {
            _rewardRepository = rewardRepository;
            _leaderboardRepository = leaderboardRepository;
            _projectRepository = projectRepository;
        }

        // ✅ Redeem a reward
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemDto redeemDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var redeemableItem = await _rewardRepository.GetRedeemableItemByIdAsync(redeemDto.RedeemableItemId, redeemDto.ProjectId);

            if (redeemableItem == null)
                return NotFound("Reward item not found for this project.");

            // Fetch total points for the user in the project (after awaiting the task)
            var leaderboard = await _leaderboardRepository.GetLeaderboardByProjectAsync(redeemDto.ProjectId);
            var userPoints = leaderboard.Where(l => l.UserId == userId).Sum(l => l.PointsEarned); // Corrected after awaiting

            if (userPoints < redeemableItem.Cost)
                return BadRequest("Not enough points to redeem this reward.");

            // Deduct points and update leaderboard
            var leaderboardEntry = await _leaderboardRepository.GetLeaderboardEntry(redeemDto.RedeemableItemId, userId);
            if (leaderboardEntry != null)
            {
                leaderboardEntry.PointsEarned -= redeemableItem.Cost;
                await _leaderboardRepository.UpdateLeaderboardEntry(leaderboardEntry);
            }

            // Save redeem history
            var redeemHistory = new RedeemHistory
            {
                UserId = userId,
                RedeemableItemId = redeemableItem.Id,
                PointsUsed = redeemableItem.Cost,
                RedeemedOn = DateTime.UtcNow,
                ProjectId = redeemDto.ProjectId
            };

            await _rewardRepository.SaveRedeemHistoryAsync(redeemHistory);

            return Ok("Reward redeemed successfully!");
        }


        // ✅ Get Redeemed Rewards History (Admin only)
        [HttpGet("redeemed/{projectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRedeemedRewards(int projectId)
        {
            var redeemedRewards = await _rewardRepository.GetRedeemedHistoryByProjectAsync(projectId);
            if (redeemedRewards == null || !redeemedRewards.Any())
                return NotFound("No redeemed rewards found for this project.");

            return Ok(redeemedRewards);
        }
    }
}
