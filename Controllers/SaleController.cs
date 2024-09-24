using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AspNetCore.Reporting;
using PlayOn24.Models;
using System.Data;

namespace PlayOn24.Controllers
{
    public class SaleController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _db;

        public SaleController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, ApplicationDbContext db)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _hostingEnvironment = hostingEnvironment;
            _db = db;
        }

        public IActionResult SellProduct(int customerId, int productId, int quantity, int sale)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("CreateSale", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@Price", sale);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("GenerateInvoice", new { customerId, productId });
        }

      

        public ActionResult GenerateInvoice(int saleId)
        {
            DataTable invoiceData = GetInvoiceData(saleId);

            string rdlcFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Reports", "InvoiceReport.rdlc");
            LocalReport localReport = new LocalReport(rdlcFilePath);

            localReport.AddDataSource("InvoiceDataSet", invoiceData);

            var result = localReport.Execute(RenderType.Pdf);

            return File(result.MainStream, "application/pdf", "Invoice.pdf");
        }

        private DataTable GetInvoiceData(int saleId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CustomerName");
            dt.Columns.Add("ProductName");
            dt.Columns.Add("Quantity");
            dt.Columns.Add("Price");
            dt.Columns.Add("Total");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                c.FirstName + ' ' + c.LastName AS CustomerName, 
                p.ProductName AS ProductName, 
                s.Quantity, 
                p.Price, 
                (s.Quantity * p.Price) AS Total
            FROM Sales s
            INNER JOIN Customers c ON s.CustomerID = c.CustomerID
            INNER JOIN Products p ON s.ProductID = p.ProductID
            WHERE s.SaleID = @SaleID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SaleID", saleId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dt.Rows.Add(
                        reader["CustomerName"].ToString(),
                        reader["ProductName"].ToString(),
                        reader["Quantity"].ToString(),
                        reader["Price"].ToString(),
                        reader["Total"].ToString()
                    );
                }
            }

            return dt;
        }



        [HttpGet]
        public IActionResult InvoiceList(string customerName = null)
        {
            List<InvoiceViewModel> invoices;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT s.SaleID, 
                   c.FirstName + ' ' + c.LastName AS CustomerName, 
                   p.ProductName AS ProductName, 
                   s.Quantity, 
                   s.TotalPrice 
            FROM Sales s
            INNER JOIN Customers c ON s.CustomerID = c.CustomerID
            INNER JOIN Products p ON s.ProductID = p.ProductID";

                // Add condition for filtering by customer name if provided
                if (!string.IsNullOrEmpty(customerName))
                {
                    query += " WHERE c.FirstName + ' ' + c.LastName LIKE @CustomerName";
                }

                SqlCommand cmd = new SqlCommand(query, conn);

                // Add parameter only if filtering
                if (!string.IsNullOrEmpty(customerName))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", "%" + customerName + "%");
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                invoices = new List<InvoiceViewModel>();
                while (reader.Read())
                {
                    invoices.Add(new InvoiceViewModel
                    {
                        InvoiceID = (int)reader["SaleID"], // Adjust according to your table structure
                        CustomerName = reader["CustomerName"].ToString(),
                        ProductName = reader["ProductName"].ToString(),
                        Quantity = (int)reader["Quantity"],
                        TotalAmount = (decimal)reader["TotalPrice"]
                    });
                }
            }

            ViewBag.CustomerName = customerName; // For the filter textbox
            return View(invoices);
        }




        public ActionResult InvoiceViewModels(string customerName)
        {
            var invoices = from i in _db.Invoices
                           select new InvoiceViewModel
                           {
                               InvoiceID = i.InvoiceID,
                               InvoiceNumber = i.InvoiceNumber,
                               CustomerName = i.Customer.FirstName + " " + i.Customer.LastName,
                               Date = i.Date,
                               TotalAmount = i.TotalAmount
                           };

            if (!String.IsNullOrEmpty(customerName))
            {
                invoices = invoices.Where(i => i.CustomerName.Contains(customerName));
            }

            ViewBag.CustomerName = customerName;
            return View(invoices.ToList());
        }

    }
}
