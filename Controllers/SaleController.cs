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

        public IActionResult SellProduct(int customerId, int productId, int quantity)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("CreateSale", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
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
                p.Name AS ProductName, 
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



        public IActionResult InvoiceList(int? customerId)
        {
            List<Sale> sales;
            using (SqlConnection conn = new SqlConnection("DefaultConnection"))
            {
                string query = "SELECT * FROM Sales";
                if (customerId != null)
                {
                    query += " WHERE CustomerID = @CustomerID";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                if (customerId != null)
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                sales = new List<Sale>();
                while (reader.Read())
                {
                    sales.Add(new Sale
                    {
                        SaleID = (int)reader["SaleID"],
                        CustomerID = (int)reader["CustomerID"],
                        ProductID = (int)reader["ProductID"],
                        SaleDate = (DateTime)reader["SaleDate"],
                        Quantity = (int)reader["Quantity"],
                        TotalPrice = (decimal)reader["TotalPrice"]
                    });
                }
            }
            return View(sales);
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
