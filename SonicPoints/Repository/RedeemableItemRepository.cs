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

        // ✅ Add a new redeemable item to the database
        public async Task<RedeemableItem> AddRedeemableItemAsync(RedeemableItem redeemableItem)
        {
            _context.RedeemableItems.Add(redeemableItem);
            await _context.SaveChangesAsync();
            return redeemableItem;
        }

        // ✅ Get a redeemable item by its ID
        public async Task<RedeemableItem> GetRedeemableItemByIdAsync(int id)
        {
            return await _context.RedeemableItems
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // ✅ Get all redeemable items for a specific project
        public async Task<List<RedeemableItem>> GetRedeemableItemsByProjectAsync(int projectId)
        {
            return await _context.RedeemableItems
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
        }

        // ✅ Update a redeemable item
        public async Task<RedeemableItem> UpdateRedeemableItemAsync(RedeemableItem redeemableItem)
        {
            _context.RedeemableItems.Update(redeemableItem);
            await _context.SaveChangesAsync();
            return redeemableItem;
        }

        // ✅ Delete a redeemable item
        public async Task DeleteRedeemableItemAsync(int id)
        {
            var redeemableItem = await _context.RedeemableItems.FindAsync(id);
            if (redeemableItem != null)
            {
                _context.RedeemableItems.Remove(redeemableItem);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Save changes to the database
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
