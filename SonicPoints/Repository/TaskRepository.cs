using Microsoft.EntityFrameworkCore;
using SonicPoints.Data;
using SonicPoints.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get tasks by projectId
        public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId, string userId)
        {
            return await _context.Tasks
                .Where(t => t.ProjectId == projectId && (t.Project.AdminId == userId || t.Project.ProjectUsers.Any(pu => pu.UserId == userId)))
                .ToListAsync();  // Ensure you're using ToListAsync from EF Core
        }

        // ✅ Create a new task
        public async Task<TaskItem> CreateTaskAsync(TaskItem task, string userId)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Add the user to the ProjectUser list if not already part of the project
            if (!_context.ProjectUsers.Any(pu => pu.UserId == userId && pu.ProjectId == task.ProjectId))
            {
                _context.ProjectUsers.Add(new ProjectUser { UserId = userId, ProjectId = task.ProjectId, Role = "Member" });
                await _context.SaveChangesAsync();
            }

            return task;
        }

        // ✅ Update Task Status
        public async Task<bool> UpdateTaskStatusAsync(TaskItem task)
        {
            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask == null)
                return false;  // Task not found

            existingTask.Status = task.Status;
            _context.Tasks.Update(existingTask);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Get Task by ID
        public async Task<TaskItem> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        // ✅ Save changes
        public async Task<bool> SaveAsync()
        {
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
