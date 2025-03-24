using SonicPoints.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public interface IRedeemableItemRepository
    {
        Task<RedeemableItem> AddRedeemableItemAsync(RedeemableItem redeemableItem);
        Task<RedeemableItem> GetRedeemableItemByIdAsync(int id);
        Task<List<RedeemableItem>> GetRedeemableItemsByProjectAsync(int projectId);
    }
}
