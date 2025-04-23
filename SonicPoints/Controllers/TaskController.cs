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
            try
            {
                Console.WriteLine("🟢 Received Task Create Request:");
                Console.WriteLine($"Title: {createTaskDto.Title}");
                Console.WriteLine($"Priority: {createTaskDto.Priority}");
                Console.WriteLine($"DueDate: {createTaskDto.DueDate}");
                Console.WriteLine($"ProjectId: {createTaskDto.ProjectId}");
                Console.WriteLine($"Points: {createTaskDto.RewardPoints}");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!await _projectAuthorization.HasProjectRoleAsync(userId, createTaskDto.ProjectId, "Admin", "Manager"))
                    return StatusCode(403, "You are not authorized to create tasks in this project."); // ✅ fixed



                var task = new TaskItem
                {
                    Title = createTaskDto.Title,
                    Description = createTaskDto.Description,
                    Status = ProjectTaskStatus.Backlog,
                    Priority = createTaskDto.Priority,
                    ProjectId = createTaskDto.ProjectId,
                    RewardPoints = createTaskDto.RewardPoints,
                    AssignedDate = DateTime.UtcNow,
                    DueDate = createTaskDto.DueDate,
                    UserId = userId
                };

                var createdTask = await _taskRepository.CreateTaskAsync(task, userId);
                return CreatedAtAction(nameof(GetTaskById), new { taskId = createdTask.Id }, createdTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Task creation failed: " + ex.Message);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
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
        // ✅ Get task status counts for a project (for dashboard overview)
        [HttpGet("project/{projectId}/status-counts")]
        public async Task<IActionResult> GetTaskStatusCounts(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user has access to this project
            var hasAccess = await _projectAuthorization.HasProjectRoleAsync(userId, projectId, "Admin", "Manager", "Checker", "Member");
            if (!hasAccess)
                return Forbid("You do not have access to this project.");

            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);

            var counts = new
            {
                Backlog = tasks.Count(t => t.Status == ProjectTaskStatus.Backlog),
                InProgress = tasks.Count(t => t.Status == ProjectTaskStatus.InProgress),
                Review = tasks.Count(t => t.Status == ProjectTaskStatus.Review),
                Completed = tasks.Count(t => t.Status == ProjectTaskStatus.Completed)
            };

            return Ok(counts);
        }


        [HttpPut("{taskId}")]
        public async Task<IActionResult> EditTask(int taskId, [FromBody] EditTaskDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var task = await _taskRepository.GetTaskByIdAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            // Only Admins and Managers can edit
            var allowed = await _projectAuthorization.HasProjectRoleAsync(userId, task.ProjectId, "Admin", "Manager");
            if (!allowed)
                return Forbid("Not authorized to edit task.");

            // Update fields
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate;

            var updated = await _taskRepository.UpdateTaskAsync(task);
            if (!updated)
                return BadRequest("Failed to update task.");

            return Ok(task);
        }
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderTasks([FromBody] List<TaskOrderDto> list)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedTasks = new List<TaskItem>();

            foreach (var item in list)
            {
                var task = await _taskRepository.GetTaskByIdAsync(item.TaskId);
                if (task == null) continue;

                // Assign the task to the current user if it's being moved to InProgress or Review
                if (item.NewStatus == ProjectTaskStatus.InProgress || item.NewStatus == ProjectTaskStatus.Review)
                {
                    task.UserId = userId;
                }

                // If a task is moved back to Backlog, remove assigned user
                if (item.NewStatus == ProjectTaskStatus.Backlog)
                {
                    task.UserId = null;
                }

                task.Status = item.NewStatus;
                await _taskRepository.UpdateTaskAsync(task);
                updatedTasks.Add(task);
            }

            return Ok(updatedTasks);
        }

        [HttpGet("project/{projectId}/progress-trend")]
        public async Task<IActionResult> GetProjectProgressTrend(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var hasAccess = await _projectAuthorization.HasProjectRoleAsync(userId, projectId, "Admin", "Manager", "Checker", "Member");
            if (!hasAccess)
                return Forbid();

            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);

            var trend = tasks
                .Where(t => t.Status == ProjectTaskStatus.Completed)
                .GroupBy(t => t.AssignedDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                });

            return Ok(trend);
        }



        [HttpGet("project/{projectId}/analytics")]
        public async Task<IActionResult> GetTaskAnalytics(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var hasAccess = await _projectAuthorization.HasProjectRoleAsync(userId, projectId, "Admin", "Manager");
            if (!hasAccess)
                return Forbid();

            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            var grouped = tasks
                .GroupBy(t => t.User?.Email ?? "Unassigned")
                .Select(g => new {
                    User = g.Key,
                    TaskCount = g.Count(),
                    Completed = g.Count(t => t.Status == ProjectTaskStatus.Completed)
                });

            return Ok(grouped);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var authorized = await _projectAuthorization.HasProjectRoleAsync(userId, projectId, "Admin", "Manager", "Checker", "Member");
            if (!authorized)
                return Forbid();

            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            return Ok(tasks);
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
