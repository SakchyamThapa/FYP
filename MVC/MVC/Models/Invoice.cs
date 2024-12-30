namespace MVC.Models
{
    public class Invoice
    {
        // Primary Key
        public int Id { get; set; }

        // Invoice Information
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        // Customer Information
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }

        // Billing Details
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Status and Notes
        public string Status { get; set; } = "Pending"; // e.g., "Paid", "Pending", "Overdue"
        public string Notes { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
