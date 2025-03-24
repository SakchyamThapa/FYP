using SonicPoints.Dto.SonicPoints.DTOs;
using SonicPoints.Models;

namespace SonicPoints.Repositories
{
    public interface ITaskRepository
    {
        Task<IQueryable<TaskItem>> GetTasksByProjectIdAsync(int projectId, string userId);
        Task<TaskItem> CreateTaskAsync(TaskItem task, string userId);
        Task<bool> UpdateTaskStatusAsync(int taskId, string userId, UpdateTaskDto updateTaskDto);
        Task<bool> CheckTaskCompletionAsync(int taskId, string userId);
    }
}
