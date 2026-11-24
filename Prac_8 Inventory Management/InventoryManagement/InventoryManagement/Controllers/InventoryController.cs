using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IConfiguration _configuration;

        public InventoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Database Connection
        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));
        }

        // READ: Display All Products and Search Products
        public IActionResult Index(string searchString)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection con = GetConnection())
            {
                string query = @"SELECT * FROM Products
                                 WHERE ProductName LIKE @Search
                                 OR Category LIKE @Search";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@Search",
                    "%" + (searchString ?? "") + "%"
                );

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        ProductId = Convert.ToInt32(reader["ProductId"]),
                        ProductName = reader["ProductName"].ToString()!,
                        Category = reader["Category"].ToString()!,
                        Price = Convert.ToDecimal(reader["Price"]),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                        Supplier = reader["Supplier"].ToString()!
                    });
                }
            }

            ViewBag.SearchString = searchString;

            return View(products);
        }

        // CREATE: Display Add Product Form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE: Insert Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            using (SqlConnection con = GetConnection())
            {
                string query = @"INSERT INTO Products
                (ProductName, Category, Price, Quantity, Supplier)
                VALUES
                (@ProductName, @Category, @Price, @Quantity, @Supplier)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Category", product.Category);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                cmd.Parameters.AddWithValue("@Supplier", product.Supplier);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Product added successfully!";

            return RedirectToAction("Index");
        }

        // UPDATE: Display Edit Form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Product product = new Product();

            using (SqlConnection con = GetConnection())
            {
                string query = "SELECT * FROM Products WHERE ProductId=@id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    product.ProductId = Convert.ToInt32(reader["ProductId"]);
                    product.ProductName = reader["ProductName"].ToString()!;
                    product.Category = reader["Category"].ToString()!;
                    product.Price = Convert.ToDecimal(reader["Price"]);
                    product.Quantity = Convert.ToInt32(reader["Quantity"]);
                    product.Supplier = reader["Supplier"].ToString()!;
                }
                else
                {
                    return NotFound();
                }
            }

            return View(product);
        }

        // UPDATE: Save Edited Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            using (SqlConnection con = GetConnection())
            {
                string query = @"UPDATE Products SET
                ProductName=@ProductName,
                Category=@Category,
                Price=@Price,
                Quantity=@Quantity,
                Supplier=@Supplier
                WHERE ProductId=@ProductId";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Category", product.Category);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                cmd.Parameters.AddWithValue("@Supplier", product.Supplier);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Product updated successfully!";

            return RedirectToAction("Index");
        }

        // DELETE: Remove Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "DELETE FROM Products WHERE ProductId=@id";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Product deleted successfully!";

            return RedirectToAction("Index");
        }
    }
}