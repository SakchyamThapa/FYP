using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.Dto;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonicPoints.Controllers
{
    [Route("api/redeemableitems")]
    [ApiController]
    public class RedeemableItemController : ControllerBase
    {
        private readonly IRedeemableItemRepository _redeemableItemRepository;

        // Injecting the repository for managing redeemable items
        public RedeemableItemController(IRedeemableItemRepository redeemableItemRepository)
        {
            _redeemableItemRepository = redeemableItemRepository;
        }

        [HttpPost("add")]
        //[Authorize(Roles = "Admin")]  // Only Admins can add redeemable items
        public async Task<IActionResult> AddRedeemableItem([FromBody] RedeemableItem redeemableItem)
        {
            if (redeemableItem == null)
                return BadRequest("Redeemable item data is invalid.");

            // Check for ModelState errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the redeemable item data
            if (string.IsNullOrEmpty(redeemableItem.Name) || redeemableItem.Cost <= 0 || redeemableItem.ProjectId <= 0)
            {
                return BadRequest("Invalid redeemable item data.");
            }

            // Save the redeemable item in the database
            var addedItem = await _redeemableItemRepository.AddRedeemableItemAsync(redeemableItem);

            if (addedItem == null)
                return StatusCode(500, "An error occurred while adding the redeemable item.");

            // Convert the added item to DTO and return the response
            var redeemableItemDto = new RedeemableItemDto
            {
                Id = addedItem.Id,
                Name = addedItem.Name,
                Cost = addedItem.Cost,
                ProjectId = addedItem.ProjectId
            };

            return CreatedAtAction(nameof(GetRedeemableItemById), new { id = redeemableItemDto.Id }, redeemableItemDto);
        }



        // GET: api/redeemableitems/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRedeemableItemById(int id)
        {
            var redeemableItem = await _redeemableItemRepository.GetRedeemableItemByIdAsync(id);

            if (redeemableItem == null)
                return NotFound("Redeemable item not found.");

            // Convert to DTO and return the response
            var redeemableItemDto = new RedeemableItemDto
            {
                Id = redeemableItem.Id,
                Name = redeemableItem.Name,
                Cost = redeemableItem.Cost,
                ProjectId = redeemableItem.ProjectId,
            };

            return Ok(redeemableItemDto);
        }

        // GET: api/redeemableitems/project/{projectId}
        [HttpGet("project/{projectId}")]
        [Authorize]
        public async Task<IActionResult> GetRedeemableItemsByProject(int projectId)
        {
            // Fetch all redeemable items for the given project
            var redeemableItems = await _redeemableItemRepository.GetRedeemableItemsByProjectAsync(projectId);

            if (redeemableItems == null || redeemableItems.Count == 0)
                return NotFound("No redeemable items found for this project.");

            // Convert models to DTOs
            var result = redeemableItems.Select(r => new RedeemableItemDto
            {
                Id = r.Id,
                Name = r.Name,
                Cost = r.Cost,
                ProjectId = r.ProjectId,
            }).ToList();

            return Ok(result);
        }
    }
}
