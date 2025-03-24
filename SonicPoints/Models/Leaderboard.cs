namespace SonicPoints.Models
{
    public class Leaderboard
    {

        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskItem Task { get; set; }
        public string UserId { get; set; } // User who completed it
        public User User { get; set; }
        public DateTime CompletedOn { get; set; }


    }
}