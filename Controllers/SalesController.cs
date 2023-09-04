using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace VB_ecommerce_backend.Controllers;

[ApiController]    
[Route("[controller]")]
public class SalesController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly MySqlConnection _connection;

    public SalesController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
    }
    
    [HttpPost]
    public async Task<IActionResult> AddProducts(List<ProductModel> products)
    {
        if (products == null || products.Count == 0)
        {
            return BadRequest("Boş veya geçersiz veri.");
        }

      
        _connection.Open();
            foreach (var product in products)
            {
                string insertQuery = "INSERT INTO sales (product_category, product_id, customer_id) VALUES (@product_category, @product_id, @customer_id)";
                MySqlCommand cmd = new MySqlCommand(insertQuery, _connection);
                cmd.Parameters.AddWithValue("@product_category", product.product_category);
                cmd.Parameters.AddWithValue("@product_id", product.product_id);
                cmd.Parameters.AddWithValue("@customer_id", product.customer_id);
                cmd.ExecuteNonQuery();
                
                if (product.product_category == 1)
                {
                    string updateQuery = "UPDATE products_computer SET stock = stock - 1 WHERE id = @product_id";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, _connection);
                    updateCmd.Parameters.AddWithValue("@product_id", product.product_id);
                    updateCmd.ExecuteNonQuery();
                }
                // Eğer product_category 2 ise products_earphones tablosunda stok azaltma
                else if (product.product_category == 2)
                {
                    string updateQuery = "UPDATE products_earphones SET stock = stock - 1 WHERE id = @product_id";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, _connection);
                    updateCmd.Parameters.AddWithValue("@product_id", product.product_id);
                    updateCmd.ExecuteNonQuery();
                }
            }
        

        return Ok("Veriler başarıyla eklendi.");
    }

    public class ProductModel
    {
        public int product_category { get; set; }
        public int product_id { get; set; }
        public int customer_id { get; set; }
    }
}