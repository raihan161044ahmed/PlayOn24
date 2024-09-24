namespace PlayOn24.Models
{
    public class Sale
    {
        public int SaleID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public int price { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerName { get; set; }
    }
}
