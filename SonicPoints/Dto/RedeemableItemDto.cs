namespace SonicPoints.Dto
{
    public class RedeemableItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }  // Points required to redeem
        public int ProjectId { get; set; } // Project ID
    }
}
