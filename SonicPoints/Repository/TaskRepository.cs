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

        public async Task<bool> UpdateTaskStatusAsync(int taskId, string userId, UpdateTaskDto updateTaskDto)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return false; // Task not found
            }

            // Ensure the task has an associated project
            if (task.Project == null)
            {
                return false; // Task does not belong to a valid project
            }

            // Ensure the user is an admin of the project
            if (task.Project.AdminId != userId)
            {
                return false; // Unauthorized user
            }

            // Validate the status as a valid enum value
            if (!Enum.IsDefined(typeof(SonicPoints.Models.TaskStatus), updateTaskDto.Status))
            {
                return false; // Invalid status value
            }

            // Convert integer to TaskStatus enum
            task.Status = (SonicPoints.Models.TaskStatus)updateTaskDto.Status;

            // Handle specific task status transitions
            if (task.Status == SonicPoints.Models.TaskStatus.Completed || task.Status == SonicPoints.Models.TaskStatus.Review)
            {
                task.AssignedDate = DateTime.UtcNow;
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            return true;
        }




        public async Task<bool> CheckTaskCompletionAsync(int taskId, string userId)
        {
            // Fetch the task with associated project data
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Status == Models.TaskStatus.Review);

            if (task == null)
                return false; // Task not found or not in review status

            // Fetch the role of the user in the project
            var userRole = await _context.ProjectUsers
                .Where(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            // Allow only Admins or Checkers to mark the task as completed
            if (task.Project.AdminId != userId && userRole != "Checker")
                return false; // Unauthorized user

            // Proceed with marking task as completed
            task.Status = Models.TaskStatus.Completed;

            // Add leaderboard entry for task completion
            var taskCompletion = new Leaderboard
            {
                TaskId = task.Id,
                UserId = userId,
               
            };

            _context.Leaderboards.Add(taskCompletion);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
