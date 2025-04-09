using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.Dto;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SonicPoints.Controllers
{
    [Route("api/redeemableitems")]
    [ApiController]
    [Authorize]
    public class RedeemableItemController : ControllerBase
    {
        private readonly IRedeemableItemRepository _redeemableItemRepository;
        private readonly IProjectRepository _projectRepository;

        public RedeemableItemController(IRedeemableItemRepository redeemableItemRepository, IProjectRepository projectRepository)
        {
            _redeemableItemRepository = redeemableItemRepository;
            _projectRepository = projectRepository;
        }

        // ✅ POST: api/redeemableitems (Create a new redeemable item for a project)
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can create redeemable items
        public async Task<IActionResult> CreateRedeemableItem([FromBody] RedeemableItemDto redeemableItemDto)
        {
            // Ensure the project exists
            var project = await _projectRepository.GetProjectByIdAsync(redeemableItemDto.ProjectId, User.FindFirstValue("sub"));
            if (project == null)
                return NotFound("Project not found.");

            var redeemableItem = new RedeemableItem
            {
                Name = redeemableItemDto.Name,
                Cost = redeemableItemDto.Cost,
                ProjectId = redeemableItemDto.ProjectId
            };

            var createdItem = await _redeemableItemRepository.AddRedeemableItemAsync(redeemableItem);

            return CreatedAtAction(nameof(GetRedeemableItemById), new { id = createdItem.Id }, createdItem);
        }

        // ✅ GET: api/redeemableitems/project/{projectId} (Get all redeemable items for a specific project)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetRedeemableItemsByProject(int projectId)
        {
            var redeemableItems = await _redeemableItemRepository.GetRedeemableItemsByProjectAsync(projectId);

            if (redeemableItems == null || redeemableItems.Count == 0)
                return NotFound("No redeemable items found for this project.");

            return Ok(redeemableItems);
        }

        // ✅ GET: api/redeemableitems/{id} (Get redeemable item by ID)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRedeemableItemById(int id)
        {
            var redeemableItem = await _redeemableItemRepository.GetRedeemableItemByIdAsync(id);

            if (redeemableItem == null)
                return NotFound("Redeemable item not found.");

            return Ok(redeemableItem);
        }

        // ✅ PUT: api/redeemableitems/{id} (Update a redeemable item by ID)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can update redeemable items
        public async Task<IActionResult> UpdateRedeemableItem(int id, [FromBody] RedeemableItemDto redeemableItemDto)
        {
            var redeemableItem = await _redeemableItemRepository.GetRedeemableItemByIdAsync(id);
            if (redeemableItem == null)
                return NotFound("Redeemable item not found.");

            redeemableItem.Name = redeemableItemDto.Name;
            redeemableItem.Cost = redeemableItemDto.Cost;

            var updatedItem = await _redeemableItemRepository.UpdateRedeemableItemAsync(redeemableItem);

            return Ok(updatedItem);
        }

        // ✅ DELETE: api/redeemableitems/{id} (Delete a redeemable item by ID)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete redeemable items
        public async Task<IActionResult> DeleteRedeemableItem(int id)
        {
            var redeemableItem = await _redeemableItemRepository.GetRedeemableItemByIdAsync(id);
            if (redeemableItem == null)
                return NotFound("Redeemable item not found.");

            await _redeemableItemRepository.DeleteRedeemableItemAsync(id);

            return NoContent();
        }
    }
}
