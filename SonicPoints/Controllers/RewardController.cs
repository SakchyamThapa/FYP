using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using SonicPoints.Services;
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
        private readonly IProjectAuthorizationService _projectAuthorization;

        public RewardController(
            IRewardRepository rewardRepository,
            ILeaderboardRepository leaderboardRepository,
            IProjectRepository projectRepository,
            IProjectAuthorizationService projectAuthorization)
        {
            _rewardRepository = rewardRepository;
            _leaderboardRepository = leaderboardRepository;
            _projectRepository = projectRepository;
            _projectAuthorization = projectAuthorization;
        }

        // ✅ Redeem a reward
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemDto redeemDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!await _projectAuthorization.HasProjectRoleAsync(userId, redeemDto.ProjectId, "Admin", "Manager", "Member"))
                return Forbid("You are not authorized to redeem rewards in this project.");

            var redeemableItem = await _rewardRepository.GetRedeemableItemByIdAsync(redeemDto.RedeemableItemId, redeemDto.ProjectId);
            if (redeemableItem == null)
                return NotFound("Reward item not found for this project.");

            var leaderboard = await _leaderboardRepository.GetLeaderboardByProjectAsync(redeemDto.ProjectId);
            var userEntries = leaderboard.Where(l => l.UserId == userId).OrderByDescending(l => l.PointsEarned).ToList();
            var userPoints = userEntries.Sum(l => l.PointsEarned);

            if (userPoints < redeemableItem.Cost)
                return BadRequest("Not enough points to redeem this reward.");

            int remainingCost = redeemableItem.Cost;
            foreach (var entry in userEntries)
            {
                if (remainingCost <= 0) break;

                int deduct = System.Math.Min(entry.PointsEarned, remainingCost);
                entry.PointsEarned -= deduct;
                remainingCost -= deduct;

                await _leaderboardRepository.UpdateLeaderboardEntry(entry);
            }

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

        // ✅ Get Redeemed Rewards History (project-admin only)
        [HttpGet("redeemed/{projectId}")]
        public async Task<IActionResult> GetRedeemedRewards(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!await _projectAuthorization.HasProjectRoleAsync(userId, projectId, "Admin"))
                return Forbid("Only project Admins can access redemption history.");

            var redeemedRewards = await _rewardRepository.GetRedeemedHistoryByProjectAsync(projectId);
            if (redeemedRewards == null || !redeemedRewards.Any())
                return NotFound("No redeemed rewards found for this project.");

            return Ok(redeemedRewards);
        }
    }
}
