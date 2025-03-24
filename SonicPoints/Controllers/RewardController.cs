using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.Data;
using SonicPoints.Dto;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SonicPoints.Controllers
{
    [Route("api/rewards")]
    [ApiController]
    public class RewardController : ControllerBase
    {
        private readonly IRewardRepository _rewardRepository;
        private readonly AppDbContext _context;

        public RewardController(IRewardRepository rewardRepository, AppDbContext context)
        {
            _rewardRepository = rewardRepository;
            _context = context;
        }

        // ✅ Redeem Reward API
        [HttpPost("redeem")]
        [Authorize]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemDto redeemDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Validate reward item
            var redeemableItem = await _rewardRepository.GetRedeemableItemByIdAsync(redeemDto.RedeemableItemId, redeemDto.ProjectId);
            if (redeemableItem == null)
                return NotFound("Reward item not found for this project.");

            if (redeemableItem.Cost <= 0)
                return BadRequest("Invalid reward cost.");

            // Fetch user points from Leaderboard
            var userPoints = _context.Leaderboards
                .Where(l => l.UserId == userId && l.Task.ProjectId == redeemDto.ProjectId)
                .Sum(l => l.PointsEarned);

            // Check if the user has enough points
            if (userPoints < redeemableItem.Cost)
                return BadRequest("Not enough points to redeem this reward.");

            // Deduct points from Leaderboard
            var leaderboardEntry = await _context.Leaderboards
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Task.ProjectId == redeemDto.ProjectId);

            if (leaderboardEntry != null)
            {
                leaderboardEntry.PointsEarned -= redeemableItem.Cost;
                _context.Leaderboards.Update(leaderboardEntry);
                await _context.SaveChangesAsync();
            }

            // Save redemption history
            var redemptionHistory = new RedeemHistory
            {
                UserId = userId,
                RedeemableItemId = redeemableItem.Id,
                PointsUsed = redeemableItem.Cost,
                RedeemedOn = DateTime.UtcNow,
                ProjectId = redeemDto.ProjectId
            };

            await _rewardRepository.SaveRedeemHistoryAsync(redemptionHistory);

            return Ok("Reward redeemed successfully!");
        }

        // ✅ Get Redeemed Rewards API (Admin Only)
        [HttpGet("redeemed/{projectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRedeemedRewards(int projectId)
        {
            try
            {
                // Validate projectId
                if (projectId <= 0)
                    return BadRequest("Invalid project ID.");

                // Fetch redeemed rewards
                var redeemedRewards = await _rewardRepository.GetRedeemedHistoryByProjectAsync(projectId);

                // Check if any rewards exist
                if (redeemedRewards == null || !redeemedRewards.Any())
                    return NotFound("No redeemed rewards found for this project.");

                // Map data to DTO
                var result = redeemedRewards.Select(r => new RedeemDto
                {
                    RedeemableItemId = r.RedeemableItemId,
                    RedeemableItemName = r.RedeemableItem?.Name ?? "Unknown",
                    PointsUsed = r.PointsUsed,
                    RedeemedAt = r.RedeemedOn,
                    UserId = r.UserId,
                    ProjectId = r.ProjectId
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error (optional, replace with your logging system)
                Console.WriteLine($"Error in GetRedeemedRewards: {ex.Message}");

                return StatusCode(500, "An error occurred while fetching redeemed rewards.");
            }
        }

    }
}
