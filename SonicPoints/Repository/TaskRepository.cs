using Microsoft.EntityFrameworkCore;
using SonicPoints.Data;
using SonicPoints.Dto.SonicPoints.DTOs;
using SonicPoints.Models;
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
        public async Task<IQueryable<TaskItem>> GetTasksByProjectIdAsync(int projectId, string userId)
        {
            return _context.Tasks
                .Where(t => t.ProjectId == projectId && (t.Project.AdminId == userId || t.Project.ProjectUsers.Any(pu => pu.UserId == userId)))  // Accessing ProjectUsers correctly
                .AsQueryable();
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

        // ✅ Update task status
        public async Task<bool> UpdateTaskStatusAsync(int taskId, string userId, UpdateTaskDto updateTaskDto)
        {
            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null || task.Project.AdminId != userId) return false;

            task.Status = (SonicPoints.Models.TaskStatus)Enum.Parse(typeof(SonicPoints.Models.TaskStatus), updateTaskDto.Status);  // Full namespace path

            if (task.Status == SonicPoints.Models.TaskStatus.Done || task.Status == SonicPoints.Models.TaskStatus.Checking)
            {
                task.AssignedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Confirm task completion (by a Checker like QA or Manager)
        public async Task<bool> CheckTaskCompletionAsync(int taskId, string userId)
        {
            var task = await _context.Tasks.Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Status == SonicPoints.Models.TaskStatus.Checking);  // Full namespace path

            var userRole = _context.ProjectUsers
                .Where(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId)
                .Select(pu => pu.Role)
                .FirstOrDefault();

            if (task == null || userRole != "Checker") return false;

            task.Status = SonicPoints.Models.TaskStatus.Done;
            task.Leaderboards.Add(new Leaderboard { TaskId = taskId, UserId = userId, CompletedOn = DateTime.UtcNow });

            // Update points for the user who completed the task
            var taskCompletion = new Leaderboard
            {
                TaskId = task.Id,
                UserId = userId,
                CompletedOn = DateTime.UtcNow
            };

            _context.Leaderboards.Add(taskCompletion);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
