using SonicPoints.DTOs;
using SonicPoints.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonicPoints.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId, string userId);
        Task<TaskItem> CreateTaskAsync(TaskItem task, string userId);
        Task<bool> UpdateTaskStatusAsync(TaskItem task);
        Task<TaskItem> GetTaskByIdAsync(int taskId);
        Task<bool> SaveAsync();
    }
}
