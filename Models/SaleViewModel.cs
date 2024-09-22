namespace PlayOn24.Models
{
    public class SaleViewModel
    {
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }

        public IEnumerable<Customer> Customers { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }

}
