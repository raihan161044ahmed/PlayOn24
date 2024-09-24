namespace PlayOn24.Models
{
    public class InvoiceViewModel
    {
        public int InvoiceID { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? CustomerName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

}
