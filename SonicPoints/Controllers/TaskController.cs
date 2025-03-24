using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonicPoints.Dto.SonicPoints.DTOs;
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

        public TaskController(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        // ✅ GET: api/tasks (Get all tasks for a specific project)
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectTasks(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId, userId);

            if (tasks == null)
                return NotFound("No tasks found for this project.");

            return Ok(tasks);
        }

        // ✅ POST: api/tasks (Create a new task for a project)
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Status = SonicPoints.Models.TaskStatus.ToDo,  // Ensure full namespace path
                ProjectId = createTaskDto.ProjectId,
                RewardPoints = createTaskDto.RewardPoints,
                AssignedDate = DateTime.UtcNow,
                DueDate = createTaskDto.DueDate
            };

            var createdTask = await _taskRepository.CreateTaskAsync(task, userId);

            return CreatedAtAction(nameof(GetProjectTasks), new { projectId = createdTask.ProjectId }, createdTask);
        }

        // ✅ PUT: api/tasks/{taskId} (Update task status, especially for checking or completing tasks)
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskDto updateTaskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _taskRepository.UpdateTaskStatusAsync(taskId, userId, updateTaskDto);

            if (!success)
                return BadRequest("Task update failed, check permissions.");

            return NoContent();
        }

        // ✅ POST: api/tasks/{taskId}/check (Checker role confirms task completion)
        [HttpPost("{taskId}/check")]
        public async Task<IActionResult> CheckTaskCompletion(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var success = await _taskRepository.CheckTaskCompletionAsync(taskId, userId);

            if (!success)
                return BadRequest("You are not authorized to check this task or the task is already completed.");

            return Ok("Task has been successfully checked.");
        }
    }
}
