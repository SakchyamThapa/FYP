namespace MVC.Models
{
    public class ReceivedEmail
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime? ReceivedDate { get; set; } // The date the email was received
    }
}
