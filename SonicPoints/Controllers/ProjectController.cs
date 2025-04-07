using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonicPoints.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize] // Requires authentication for all endpoints
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
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


        // ✅ POST: api/projects (Create a new project)
        //[Authorize]
        [HttpPost]
        
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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


        // ✅ PUT: api/projects/{id} (Update an existing project)
        [HttpPut("{id}")]
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
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _projectRepository.DeleteProjectAsync(id, userId);

            if (!success)
                return NotFound("Project not found or you don't have permission to delete.");

            return NoContent();
        }

        // ✅ POST: api/projects/{id}/add-user (Admin can add users to the project)
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/add-user")]
        public async Task<IActionResult> AddUserToProject(int id, [FromBody] string newUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _projectRepository.AddUserToProjectAsync(id, userId, newUserId);

            if (!success)
                return BadRequest("Failed to add user to project. Check if you're an admin.");

            return Ok("User added successfully.");
        }
    }
}
