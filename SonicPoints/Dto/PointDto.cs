namespace SonicPoints.Dto
{
    public class PointDto
    {
        public int PointsId { get; set; }
        public int UserId { get; set; }
        public int TotalPoints { get; set; }
        public DateTime LastUpdated { get; set; }
        public string User { get; set; }
    }
}
