

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SonicPoints.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize] // Requires authentication for all endpoints
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<User> _userManager;

        public ProjectController(IProjectRepository projectRepository, UserManager<User> userManager)
        {
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("❌ Invalid token: No user ID found in claims.");

            var projects = await _projectRepository.GetUserProjectsAsync(userId);
            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DueDate = p.DueDate,
                ProjectStatus = p.ProjectStatus,
                Progress = p.Progress
            });

            return Ok(projectDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var project = await _projectRepository.GetProjectByIdAsync(id, userId);

            if (project == null)
                return NotFound("Project not found or you don't have access.");

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                DueDate = project.DueDate,
                ProjectStatus = project.ProjectStatus,
                Progress = project.Progress
            };

            return Ok(projectDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token.");

            var project = new Project
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description,
                DueDate = createProjectDto.DueDate,
                AdminId = userId,
                ProjectStatus = "Not Started"
            };

            var createdProject = await _projectRepository.CreateProjectAsync(project, userId);

            var projectDto = new ProjectDto
            {
                Id = createdProject.Id,
                Name = createdProject.Name,
                Description = createdProject.Description,
                DueDate = createdProject.DueDate,
                ProjectStatus = createdProject.ProjectStatus,
                Progress = createdProject.Progress
            };

            return CreatedAtAction(nameof(GetProject), new { id = projectDto.Id }, projectDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto updateProjectDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var project = await _projectRepository.UpdateProjectAsync(id, userId, updateProjectDto);

            if (project == null)
                return NotFound("Project not found or you don't have permission to update.");

            return Ok(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                DueDate = project.DueDate,
                ProjectStatus = project.ProjectStatus,
                Progress = project.Progress
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _projectRepository.DeleteProjectAsync(id, userId);

            if (!success)
                return NotFound("Project not found or you don't have permission to delete.");

            return NoContent();
        }

        [HttpPost("{id}/add-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserToProject(int id, [FromBody] string userEmail)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _projectRepository.AddUserToProjectAsync(id, adminId, userEmail);

            if (!success)
                return BadRequest("Failed to add user to project. Check if the email is valid and you're an admin.");

            return Ok("User added successfully.");
        }

        [HttpPost("{id}/assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoleToUser(int id, [FromBody] AssignUserRoleDto assignUserRoleDto)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id, assignUserRoleDto.AdminId);
            if (project == null)
                return NotFound("Project not found or you don't have permission to add users.");

            var user = await _userManager.FindByIdAsync(assignUserRoleDto.UserId);
            if (user == null)
                return NotFound("User not found.");

            var success = await _projectRepository.AddUserToProjectAsync(id, assignUserRoleDto.AdminId, assignUserRoleDto.UserId);

            if (!success)
                return BadRequest("Failed to add user to project.");

            return Ok("User role assigned successfully.");
        }
    }
}
