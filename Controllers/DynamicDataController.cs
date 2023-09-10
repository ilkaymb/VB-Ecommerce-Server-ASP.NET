using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace VB_ecommerce_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class DynamicDataController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly MySqlConnection _connection;

    public DynamicDataController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connection =
            new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");

    }


        [HttpGet("GetData/{tableName}")]
        public async Task<IActionResult> GetAllData(string tableName)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return BadRequest();
                }

               
                    await _connection.OpenAsync();

                    string query = $"SELECT * FROM {tableName}";
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                        while (await reader.ReadAsync())
                        {
                            var rowData = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                rowData[reader.GetName(i)] = reader[i];
                            }
                            results.Add(rowData);
                        }
                        return Ok(results);
                    }
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }

        [HttpPost("AddData/{tableName}")]
        public async Task<IActionResult> AddData(string tableName, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName) || data == null)
                {
                    return BadRequest();
                }

               
                    await _connection.OpenAsync();

                    string columnNames = string.Join(", ", data.Keys);
                    string parameterNames = string.Join(", ", data.Keys).Replace(", ", ", @");
                    string query = $"INSERT INTO {tableName} ({columnNames}) VALUES (@{parameterNames})";

                    MySqlCommand cmd = new MySqlCommand(query, _connection);

                    foreach (var kvp in data)
                    {
                        cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                    }

                    await cmd.ExecuteNonQueryAsync();
                    return Ok("Veri eklendi");
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("DeleteData/{tableName}/{id}")]
        public async Task<IActionResult> DeleteData(string tableName, int id)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName) || id <= 0)
                {
                    return BadRequest();
                }

               
                    await _connection.OpenAsync();

                    string query = $"DELETE FROM {tableName} WHERE Id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    cmd.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return Ok($"Veri silindi: {id}");
                    }
                    else
                    {
                        return NotFound($"ID {id} ile veri bulunamadı.");
                    
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
        [HttpGet("GetData/{tableName}/{id}")]
        public async Task<IActionResult> GetData(string tableName, int id)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName) || id <= 0)
                {
                    return BadRequest();
                }

              
                    await _connection.OpenAsync();

                    string query = $"SELECT * FROM {tableName} WHERE Id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            var rowData = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                rowData[reader.GetName(i)] = reader[i];
                            }
                            return Ok(rowData);
                        }
                        else
                        {
                            return NotFound($"ID {id} ile veri bulunamadı.");
                        }
                    
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
        [HttpPut("UpdateData/{tableName}/{id}")]
        public async Task<IActionResult> UpdateData(string tableName, int id, [FromBody] Dictionary<string, object> newData)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName) || id <= 0 || newData == null)
                {
                    return BadRequest();
                }

             
                    await _connection.OpenAsync();

                    // Veriyi güncelleme sorgusu
                    string updateQuery = $"UPDATE {tableName} SET ";

                    foreach (var kvp in newData)
                    {
                        updateQuery += $"{kvp.Key} = @{kvp.Key}, ";
                    }

                    updateQuery = updateQuery.TrimEnd(',', ' ') + $" WHERE Id = @Id";
                    MySqlCommand cmd = new MySqlCommand(updateQuery, _connection);
                    cmd.Parameters.AddWithValue("@Id", id);

                    foreach (var kvp in newData)
                    {
                        cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                    }

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return Ok($"ID {id} ile veri güncellendi.");
                    }
                    else
                    {
                        return NotFound($"ID {id} ile veri bulunamadı.");
                    }
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
        [HttpGet("GetColumns/{tableName}")]
        public IActionResult GetColumns(string tableName)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return BadRequest();
                }

              
                    _connection.Open();

                    // Tablonun sütunlarını çekmek için bir sorgu oluşturun
                    string query = $"DESCRIBE {tableName}";
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    List<ColumnDetail> columns = new List<ColumnDetail>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ColumnDetail column = new ColumnDetail
                            {
                                Name = reader.GetString(0), // Sütun adı
                                Type = reader.GetString(1)  // Sütun türü
                            };
                            columns.Add(column);
                        }
                    }


                    return Ok(columns);
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            try
            {
                // Connection String to connect with MySQL database.
                string connString = "server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme";
                MySqlConnection conn = new MySqlConnection(connString);

                conn.Open();

                // Kategori adlarını almak için sorguyu oluşturun
                string getCategoryNamesSql = "SELECT category_name FROM categories";

                MySqlCommand cmd = new MySqlCommand(getCategoryNamesSql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                List<string> categoryNames = new List<string>();

                while (reader.Read())
                {
                    // Her satırdaki kategori adını alın
                    string categoryName = reader.GetString("category_name");
                    categoryNames.Add(categoryName);
                }

                conn.Close();

                return Ok(categoryNames);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        
        
        
        
        public class ColumnDetail
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
 
}