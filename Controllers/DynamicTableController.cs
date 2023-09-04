using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace VB_ecommerce_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class DynamicTableController : Controller
{
    private readonly IConfiguration _configuration;

    public DynamicTableController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDynamicTable([FromBody] CreateTableRequest request)
    {
        if (request == null)
        {
            return BadRequest("Geçersiz veri.");
        }

        try
        {
            string connectionString = _configuration.GetConnectionString("MySQLConnection"); // appsettings.json dosyasındaki bağlantı dizesini kullanın

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Kullanıcıdan gelen verilere dayalı olarak bir CREATE TABLE sorgusu oluşturun
                string createTableSql = $"CREATE TABLE {request.TableName} (Id INT AUTO_INCREMENT PRIMARY KEY, {request.ColumnDefinition})";

                using (var command = new MySqlCommand(createTableSql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                return Ok($"'{request.TableName}' adında dinamik bir tablo oluşturuldu.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Tablo oluşturulamadı: {ex.Message}");
        }
    }
}

public class CreateTableRequest
{ 
    public string TableName { get; set; }
    public string ColumnDefinition { get; set; }

    

}