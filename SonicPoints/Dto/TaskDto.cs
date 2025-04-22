using SonicPoints.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SonicPoints.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Range(0, 999)]
        public int RewardPoints { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }


    public class UpdateTaskDto
    {
        public int Status { get; set; } // 0: Backlog, 1: InProgress, 2: Review, 3: Completed
    }
    public class EditTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime DueDate { get; set; }
    }
    public class TaskOrderDto
    {
        public int TaskId { get; set; }
        public ProjectTaskStatus NewStatus { get; set; }
    }


}
