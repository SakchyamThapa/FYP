namespace SonicPoints.Dto
{
    public class RedeemDto
    {
        public int RedeemId { get; set; }
        public string RewardName { get; set; }
        public string RedeemPoints { get; set; }
        public int PointsUsed { get; set; }
        public DateTime RedeemedAt { get; set; }
        public int UserId { get; set; }
    }
}
