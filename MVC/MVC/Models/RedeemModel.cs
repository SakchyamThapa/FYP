namespace MVC.Models
{
    public class RedeemModel
    {
        public int Id { get; set; }
        public string Username { get; set; } // Username of the user redeeming points
        public int Points { get; set; }      // Redeemed points
        public int UserId { get; set; }      // Foreign key linking to the UserViewModel's Id
        public IEnumerable<RedeemModel> RedeemableItems { get; set; }
        
    }
}
