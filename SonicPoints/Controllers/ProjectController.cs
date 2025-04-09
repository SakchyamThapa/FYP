using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SonicPoints.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize] // Requires authentication for all endpoints
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<User> _userManager;  // Injecting UserManager

        public ProjectController(IProjectRepository projectRepository, UserManager<User> userManager)
        {
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        // ✅ GET: api/projects (Get all projects for the logged-in user)
        [HttpGet]
        public async Task<IActionResult> GetUserProjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

        // ✅ GET: api/projects/{id} (Get a specific project by ID)
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
        [Authorize]  // Only authenticated users can create projects
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = new Project
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description,
                DueDate = createProjectDto.DueDate,
                AdminId = userId,  // Assign the user as the admin of the project
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



        // ✅ PUT: api/projects/{id} (Update an existing project) - Admin and Manager roles can update a project
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

        // ✅ DELETE: api/projects/{id} (Delete a project, only Admins can)
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
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Get the adminId (logged-in user)

            // Call the repository method to add the user by email
            var success = await _projectRepository.AddUserToProjectAsync(id, adminId, userEmail);

            if (!success)
                return BadRequest("Failed to add user to project. Check if the email is valid and you're an admin.");

            return Ok("User added successfully.");
        }




        // ✅ POST: api/projects/{id}/assign-role (Assign roles to users in a project) - Admin only
        [HttpPost("{id}/assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoleToUser(int id, [FromBody] AssignUserRoleDto assignUserRoleDto)
        {
            // Retrieve the project by ID
            var project = await _projectRepository.GetProjectByIdAsync(id, assignUserRoleDto.AdminId);

            if (project == null)
                return NotFound("Project not found or you don't have permission to add users.");

            // Retrieve the user by UserId
            var user = await _userManager.FindByIdAsync(assignUserRoleDto.UserId);
            if (user == null)
                return NotFound("User not found.");

            // Now, we pass the correct arguments (projectId, adminId, newUserId) to AddUserToProjectAsync
            var success = await _projectRepository.AddUserToProjectAsync(id, assignUserRoleDto.AdminId, assignUserRoleDto.UserId);

            if (!success)
                return BadRequest("Failed to add user to project.");

            return Ok("User role assigned successfully.");
        }

    }
}
