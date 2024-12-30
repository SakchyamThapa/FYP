namespace MVC.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        // Foreign Key to SentEmail
        public int SentEmailId { get; set; }
        public virtual SentEmail SentEmail { get; set; }
    }
}
