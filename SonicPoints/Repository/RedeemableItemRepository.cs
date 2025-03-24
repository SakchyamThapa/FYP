using Microsoft.EntityFrameworkCore;
using SonicPoints.Data;
using SonicPoints.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public class RedeemableItemRepository : IRedeemableItemRepository
    {
        private readonly AppDbContext _context;

        public RedeemableItemRepository(AppDbContext context)
        {
            _context = context;
        }

        // Add a redeemable item to the database
        public async Task<RedeemableItem> AddRedeemableItemAsync(RedeemableItem redeemableItem)
        {
            _context.RedeemableItems.Add(redeemableItem);
            await _context.SaveChangesAsync();
            return redeemableItem;
        }

        // Fetch a redeemable item by its ID
        public async Task<RedeemableItem> GetRedeemableItemByIdAsync(int id)
        {
            return await _context.RedeemableItems
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Fetch all redeemable items for a specific project
        public async Task<List<RedeemableItem>> GetRedeemableItemsByProjectAsync(int projectId)
        {
            return await _context.RedeemableItems
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
        }
    }
}
