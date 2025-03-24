namespace SonicPoints.Dto
{
    namespace SonicPoints.DTOs
    {
        public class CreateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int ProjectId { get; set; }
            public int RewardPoints { get; set; }
            public DateTime DueDate { get; set; }
        }

        public class UpdateTaskDto
        {
            public int Status { get; set; } // "To Do", "In Progress", "Done", "Checking"
        }
    }

}