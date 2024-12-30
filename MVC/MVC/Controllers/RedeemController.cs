using Microsoft.AspNetCore.Mvc;
using MVC.Interface;
using MVC.Models;

namespace MVC.Controllers
{
    public class RedeemController : Controller
    {
        private readonly ApplicationDbContext _context;
        public IRedeemRepo _redeemRepo;

        public RedeemController(ApplicationDbContext context, IRedeemRepo redeemRepo)
        {
            _context = context;
            _redeemRepo = redeemRepo;
        }



        [HttpPost("item")]
        public IActionResult RedeemItem([FromBody] RedeemRequestModel request)
        {
            // Find the user based on the given UserId (maps to UserViewModel.Id)
            var user = _context.UserViewModels.FirstOrDefault(u => u.Id == request.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            if (user.KPIPoints < request.Cost)
            {
                return BadRequest(new { message = "Insufficient KPI Points." });
            }

            // Deduct points
            user.KPIPoints -= request.Cost;

            // Create a new redeem record
            var redemption = new RedeemModel
            {
                Username = user.FullName, // Set username for tracking
                Points = request.Cost,   // Deducted points
                UserId = user.Id         // Foreign key linking to user
            };

            // Save redeem record
            _context.Redeems.Add(redemption);
            _context.SaveChanges();

            return Ok(new { message = "Item redeemed successfully!" });
        }


        [HttpGet("history")]
        public IActionResult GetRedemptionHistory(int userId)
        {
            var redemptions = _context.Redeems
                .Where(r => r.UserId == userId)
                .ToList();

            return Ok(redemptions);
        }


    }


    public class RedeemRequestModel
    {
        public string ItemName { get; set; }
        public int Cost { get; set; }
        public int UserId { get; set; }
    }
}
