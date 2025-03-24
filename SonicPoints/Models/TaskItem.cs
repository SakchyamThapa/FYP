using System.ComponentModel.DataAnnotations;

namespace SonicPoints.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; } // "To Do", "In Progress", "Done"
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int RewardPoints { get; set; } // Points given when completed
        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; } // Task deadline
        public List<Leaderboard> Leaderboards { get; set; } // Track who completed it
    }
    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done,
        Checking
    }

}
