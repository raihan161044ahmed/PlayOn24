using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PlayOn24.Models;
using System.Data;

namespace PlayOn24.Controllers
{
    public class CustomerController : Controller
    {
        private readonly string _connectionString;

        public CustomerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult CreateCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("CreateCustomer", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", customer.LastName);
                    cmd.Parameters.AddWithValue("@Email", customer.Email);
                    cmd.Parameters.AddWithValue("@Phone", customer.Phone);
                    cmd.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value); 

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

            }
            return View(customer);
        }

    }
}
