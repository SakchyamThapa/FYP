using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.DTOs;
using SonicPoints.Models;
using SonicPoints.Repositories;
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

        public TaskController(ITaskRepository taskRepository, ILeaderboardRepository leaderboardRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _leaderboardRepository = leaderboardRepository;
            _projectRepository = projectRepository;
        }

        // ✅ Create Task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Status = ProjectTaskStatus.Backlog, // Default status is Backlog
                ProjectId = createTaskDto.ProjectId,
                RewardPoints = createTaskDto.RewardPoints,
                AssignedDate = DateTime.UtcNow,
                DueDate = createTaskDto.DueDate,
                UserId = userId // Assigned to the user who created it initially
            };

            var createdTask = await _taskRepository.CreateTaskAsync(task, userId);

            return CreatedAtAction(nameof(GetTaskById), new { taskId = createdTask.Id }, createdTask);
        }

        // ✅ Update Task Status
        [HttpPut("{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ensure the task exists and is valid to update
            var task = await _taskRepository.GetTaskByIdAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            // Check if the user has permission (Admin, Manager, or user who assigned the task)
            if (task.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
                return Unauthorized("You do not have permission to update this task.");

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

            // Ensure the task exists and is in Review status
            var task = await _taskRepository.GetTaskByIdAsync(taskId);
            if (task == null || task.Status != ProjectTaskStatus.Review)
                return BadRequest("Task not found or not in Review status.");

            // Ensure the user is authorized (Admin or Checker)
            if (!User.IsInRole("Admin") && !User.IsInRole("Checker"))
                return Unauthorized("You do not have permission to complete this task.");

            task.Status = ProjectTaskStatus.Completed;

            // Add points to the user for completing the task
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
