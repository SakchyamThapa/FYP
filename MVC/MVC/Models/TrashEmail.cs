namespace MVC.Models
{
    public class TrashEmail
    {
        public int Id { get; set; }

        // The recipient's email address
        public string To { get; set; }

        // The sender's email address
        public string From { get; set; }  // Added From property

        public string Subject { get; set; }
        public string Body { get; set; }

        // The date and time when the email was moved to trash
        public DateTime SentDate { get; set; }

        // Used to track if the email was originally from "Sent" or "Received"
        public string OriginalEmailType { get; set; }
    }
}
