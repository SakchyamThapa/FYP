namespace MVC.Models
{
    public class SentEmail
    {
        public int Id { get; set; }

        // The sender's email address (for Sent Emails)
        public string From { get; set; }

        // The recipient's email address
        public string To { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        // The date and time when the email was sent
        public DateTime SentDate { get; set; }

        public bool HasAttachment { get; set; }
    }
}
