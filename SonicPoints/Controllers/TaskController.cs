using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
using SonicPoints.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SonicPoints.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILeaderboardRepository _leaderboardRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAuthorizationService _projectAuthorization;

        public TaskController(
            ITaskRepository taskRepository,
            ILeaderboardRepository leaderboardRepository,
            IProjectRepository projectRepository,
            IProjectAuthorizationService projectAuthorization)
        {
            _taskRepository = taskRepository;
            _leaderboardRepository = leaderboardRepository;
            _projectRepository = projectRepository;
            _projectAuthorization = projectAuthorization;
        }

        // ✅ Create Task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!await _projectAuthorization.HasProjectRoleAsync(userId, createTaskDto.ProjectId, "Admin", "Manager"))
                return Forbid("You are not authorized to create tasks in this project.");

            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Status = ProjectTaskStatus.Backlog,
                ProjectId = createTaskDto.ProjectId,
                RewardPoints = createTaskDto.RewardPoints,
                AssignedDate = DateTime.UtcNow,
                DueDate = createTaskDto.DueDate,
                UserId = userId
            };

            var createdTask = await _taskRepository.CreateTaskAsync(task, userId);
            return CreatedAtAction(nameof(GetTaskById), new { taskId = createdTask.Id }, createdTask);
        }

        // ✅ Update Task Status
        [HttpPut("{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var task = await _taskRepository.GetTaskByIdAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            if (updateTaskDto.Status == (int)ProjectTaskStatus.Completed)
            {
                var allowed = await _projectAuthorization.HasProjectRoleAsync(userId, task.ProjectId, "Admin", "Manager", "Checker");
                if (!allowed)
                    return Forbid("Only Admins, Managers, or Checkers can move a task to Completed.");
            }

            task.Status = (ProjectTaskStatus)updateTaskDto.Status;
            var updated = await _taskRepository.UpdateTaskStatusAsync(task);
            if (!updated)
                return BadRequest("Failed to update task status.");

            return Ok(task);
        }

        // ✅ Check Task Completion (Admin/Checker/Manager)
        [HttpPost("{taskId}/check")]
        public async Task<IActionResult> CheckTaskCompletion(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var task = await _taskRepository.GetTaskByIdAsync(taskId);

            if (task == null || task.Status != ProjectTaskStatus.Review)
                return BadRequest("Task not found or not in Review status.");

            var authorized = await _projectAuthorization.HasProjectRoleAsync(userId, task.ProjectId, "Admin", "Manager", "Checker");
            if (!authorized)
                return Forbid("Only Admins, Managers, or Checkers can complete tasks.");

            task.Status = ProjectTaskStatus.Completed;

            var leaderboardEntry = await _leaderboardRepository.GetLeaderboardEntry(taskId, userId);
            if (leaderboardEntry == null)
            {
                leaderboardEntry = new Leaderboard
                {
                    UserId = userId,
                    TaskId = taskId,
                    PointsEarned = task.RewardPoints,
                    DateCompleted = DateTime.UtcNow
                };
                await _leaderboardRepository.AddLeaderboardEntry(leaderboardEntry);
            }
            else
            {
                leaderboardEntry.PointsEarned += task.RewardPoints;
                leaderboardEntry.DateCompleted = DateTime.UtcNow;
                await _leaderboardRepository.UpdateLeaderboardEntry(leaderboardEntry);
            }

            await _taskRepository.SaveAsync();
            return Ok("Task completed and points awarded.");
        }

        // ✅ Get Task by ID
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            var task = await _taskRepository.GetTaskByIdAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            return Ok(task);
        }
    }
}
