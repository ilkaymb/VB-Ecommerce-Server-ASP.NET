using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VB_ecommerce_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LikeController : Controller
    {
        private readonly MySqlConnection _connection;

        public LikeController()
        {
            _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
        }

        [HttpPost("AddLike/{tableName}")]
        public async Task<IActionResult> AddLike([FromBody] LikeModel likeModel, string tableName)
        {
            try
            {
                using (var connection = _connection)
                {
                    await connection.OpenAsync();

                    // Ürünü beğeni ilişkilerine ekleyin
                    await connection.ExecuteAsync(
                        "INSERT INTO like_relations (product_id, customer_id, category_id) VALUES (@product_id, @customer_id, @category_id)",
                        likeModel);

                    // Tablo adını güvenli bir şekilde sorguya eklemek için $ kullanın.
                    await connection.ExecuteAsync(
                        $"UPDATE {tableName} SET likes = likes + 1 WHERE id = @product_id",
                        new { product_id = likeModel.product_id });
                }

                return CreatedAtAction(nameof(AddLike), likeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Hata durumunda 400 Bad Request döndürün
            }
        }

        [HttpDelete("remove/{tableName}")]
        public async Task<IActionResult> RemoveLike([FromBody] LikeModel likeModel, string tableName)
        {
            try
            {
                using (var connection = _connection)
                {
                    await connection.OpenAsync();

                    // Ürünün beğeni ilişkilerinden kaldırılması
                    await connection.ExecuteAsync(
                        "DELETE FROM like_relations WHERE product_id = @product_id AND customer_id = @customer_id AND category_id=@category_id",
                        likeModel);

                    // Tablo adını güvenli bir şekilde sorguya eklemek için $ kullanın.
                    await connection.ExecuteAsync(
                        $"UPDATE {tableName} SET likes = likes - 1 WHERE id = @product_id",
                        new { product_id = likeModel.product_id });
                }

                return Ok(); // Beğeniyi kaldırıldığını belirtir.
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Hata durumunda 400 Bad Request döndürün
            }
        }

        [HttpPost("check-like")]
        public async Task<IActionResult> IsProductLiked([FromBody] LikeModel likeModel)
        {
            try
            {
                using (var connection = _connection)
                {
                    await connection.OpenAsync();

                    // Ürünün beğenilip beğenilmediğini kontrol edin
                    var result = await connection.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM like_relations WHERE product_id = @product_id AND customer_id = @customer_id AND category_id = @category_id",
                        likeModel);

                    return Ok(result > 0);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Hata durumunda 400 Bad Request döndürün
            }
        }

        [HttpGet("user-likes/{customerId}/{categoryId}/{tableName}")]
        public async Task<IActionResult> GetUserLikesWithProductDetails(int customerId, int categoryId, string tableName)
        {
            try
            {
                using (var connection = _connection)
                {
                    await connection.OpenAsync();

                    // Kullanıcının beğendiği ürünlerin kimliklerini alın
                    var likedProductIds = await connection.QueryAsync<int>(
                        "SELECT product_id FROM like_relations WHERE customer_id = @customerId AND category_id = @categoryId",
                        new { customerId, categoryId });

                    if (likedProductIds == null || !likedProductIds.AsList().Any())
                    {
                        // Kullanıcının beğendiği ürün yoksa boş bir liste döndürün
                        return Ok(new List<object>());
                    }

                    // Beğenilen ürünlerin detaylarını almak için sorguyu oluşturun
                    var query = $"SELECT * FROM {tableName} WHERE id IN @likedProductIds";

                    // Beğenilen ürünlerin detaylarını alın
                    var likedProducts = await connection.QueryAsync(query, new { likedProductIds });

                    return Ok(likedProducts);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Hata durumunda 400 Bad Request döndürün
            }
        }

        public class LikeModel
        {
            public int product_id { get; set; }
            public int customer_id { get; set; }
            public int category_id { get; set; }
        }
    }
}
