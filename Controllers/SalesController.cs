using Dapper;
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
    public async Task<IActionResult> GetProductsByCategoryAndCustomer(int category_id, int customer_id)
    {
        if (category_id != 1 && category_id != 2)
        {
            return BadRequest("Geçersiz kategori ID'si.");
        }

        _connection.Open();
        string selectQuery = "";

        if (category_id == 1)
        {
            // Bilgisayar ürünlerini çek
            selectQuery = "SELECT p.* FROM products_computer p " +
                          "INNER JOIN sales s ON p.id = s.product_id " +
                          "WHERE s.customer_id = @customer_id";
        }
        else if (category_id == 2)
        {
            // Kulaklık ürünlerini çek
            selectQuery = "SELECT p.* FROM products_earphones p " +
                          "INNER JOIN sales s ON p.id = s.product_id " +
                          "WHERE s.customer_id = @customer_id";
        }

        MySqlCommand cmd = new MySqlCommand(selectQuery, _connection);
        cmd.Parameters.AddWithValue("@customer_id", customer_id);

        using (var reader = cmd.ExecuteReader())
        {
            List<ProductModel> productList = new List<ProductModel>();

            while (reader.Read())
            {
                // Veritabanından gelen verileri ProductModel nesnelerine dönüştürün.
                ProductModel product = new ProductModel
                {
                    product_id = reader.GetInt32("id"),
                    product_category = category_id,
                    // Diğer özellikleri de burada alabilirsiniz.
                };
                productList.Add(product);
            }

            return Ok(productList);
        }
    }
[HttpGet("user-purchased/{customerId}/{categoryId}")]
public async Task<IActionResult> GetUserPurchased(int customerId, int categoryId)
{
    using (var connection = _connection)
    {
        connection.Open();

        // Kullanıcının beğendiği ürünlerin kimliklerini alın
        var likedProductIds = await connection.QueryAsync<int>(
            "SELECT product_id FROM like_relations WHERE customer_id = @customerId",
            new { customerId });

        // Beğenilen ürünlerin detaylarını products_computer veya products_earphone tablosundan alın
        string tableName = (categoryId == 1) ? "products_computer" : "products_earphones";

        var likedProducts = await connection.QueryAsync(
            $"SELECT * FROM {tableName} WHERE id IN @likedProductIds",
            new { likedProductIds });

        return Ok(likedProducts);
    }
}


    public class ProductModel
    {
        public int product_category { get; set; }
        public int product_id { get; set; }
        public int customer_id { get; set; }
    }
}