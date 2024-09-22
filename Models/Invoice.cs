using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PlayOn24.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        public int SaleID { get; set; }
        public DateTime Date { get; set; }

        public decimal TotalAmount { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;

        [ForeignKey("Customer")]
        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
