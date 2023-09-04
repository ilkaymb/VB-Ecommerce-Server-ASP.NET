using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
namespace VB_ecommerce_backend.Controllers;
[ApiController]
[Route("[controller]")]
public class LikeController : Controller
{
    private readonly MySqlConnection _connection;

    public LikeController()
    {
        _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
    }
    [HttpPost]
    public async Task<IActionResult> AddLike([FromBody] LikeModel likeModel)
    {

     
            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync(
                    "INSERT INTO like_relations (product_id, customer_id, category_id) VALUES (@product_id, @customer_id, @category_id)",
                    likeModel);
         
            if (likeModel.category_id == 1)
            {
                var updateResult = await connection.ExecuteAsync(
                    "UPDATE products_computer SET likes = likes + 1 WHERE id = @product_id",
                    new { product_id = likeModel.product_id });
            }
            else if (likeModel.category_id == 2)
            {
                var updateResult = await connection.ExecuteAsync(
                    "UPDATE products_earphones SET likes = likes + 1 WHERE id = @product_id",
                    new { product_id = likeModel.product_id });
            }
           }

        return CreatedAtAction(nameof(AddLike), likeModel);
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveLike([FromBody] LikeModel likeModel)
    {

     
            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync(
                    "DELETE FROM like_relations WHERE product_id = @product_id AND customer_id = @customer_id AND category_id=@category_id",
                    likeModel);
                
                if (likeModel.category_id == 1)
                {
                    var updateResult = await connection.ExecuteAsync(
                        "UPDATE products_computer SET likes = likes - 1 WHERE id = @product_id",
                        new { product_id = likeModel.product_id });
                }
                else if (likeModel.category_id == 2)
                {
                    var updateResult = await connection.ExecuteAsync(
                        "UPDATE products_earphones SET likes = likes - 1 WHERE id = @product_id",
                        new { product_id = likeModel.product_id });
                }
            }
        

        return Ok(); // Beğeniyi kaldırıldığını belirtir.
    }
    [HttpPost("check-like")]
    public async Task<IActionResult> IsProductLiked([FromBody] LikeModel likeModel)
    {
        using (var connection = _connection)
        {
            connection.Open();

            var result = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM like_relations WHERE product_id = @product_id AND customer_id = @customer_id AND category_id = @category_id",
                likeModel);

            return Ok(result > 0);
        }
    }
    [HttpGet("user-likes/{customerId}/{categoryId}")]
    public async Task<IActionResult> GetUserLikesWithProductDetails(int customerId, int categoryId)
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


    public class LikeModel
    {
        public int product_id { get; set; }
        public int customer_id { get; set; }
        public int category_id { get; set; }
    }
}