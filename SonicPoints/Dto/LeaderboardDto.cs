namespace SonicPoints.Dto
{
    public class LeaderboardDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; } // Assuming the User has a UserName property
        public int PointsEarned { get; set; }
        public int TaskCompletionCount { get; set; }
        public int RedeemedPoints { get; set; }
        public int LeaderboardRank { get; set; }
        public double ProjectProgress { get; set; } // In percentage
        public int RedeemablePoints { get; set; }
    }
}
