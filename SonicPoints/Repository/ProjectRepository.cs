using Microsoft.EntityFrameworkCore;
using SonicPoints.Data;
using SonicPoints.DTOs;
using SonicPoints.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetUserProjectsAsync(string userId)
        {
            return await _context.Projects
                .Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId))
                .ToListAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int projectId, string userId)
        {
            return await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.ProjectUsers.Any(pu => pu.UserId == userId));
        }

        public async Task<Project> CreateProjectAsync(Project project, string userId)
        {
            // Save the project first to generate its Id (auto-increment behavior)
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Create a ProjectUser for the project creator
            var projectUser = new ProjectUser
            {
                ProjectId = project.Id,  // This should now have the correct integer value
                UserId = userId,
                Role = "Admin",  // Set role to "Admin" for the creator
                RewardPoints = 0
            };

            // Add the ProjectUser and save
            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            return project;
        }



        public async Task<Project> UpdateProjectAsync(int projectId, string userId, UpdateProjectDto updateProjectDto)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.AdminId != userId) return null;

            project.Name = updateProjectDto.Name;
            project.Description = updateProjectDto.Description;
            project.DueDate = updateProjectDto.DueDate;

            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(int projectId, string userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.AdminId != userId) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddUserToProjectAsync(int projectId, string adminId, string newUserId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.AdminId != adminId) return false;

            var projectUser = new ProjectUser { ProjectId = projectId, UserId = newUserId, Role = "Member" };
            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
