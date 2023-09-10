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
    private readonly MySqlConnection _connection;

    public DynamicTableController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connection =
            new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");

    }

  [HttpPost]
public async Task<IActionResult> CreateDynamicTable([FromBody] CreateTableRequest request)
{
    try
    {
        // Connection String to connect with MySQL database.
        string connString = "server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme";
        MySqlConnection conn = new MySqlConnection(connString);

        conn.Open();
        string columnDefinitions = string.Join(", ", request.ColumnDefinitions.Select(c => $"{c.Name} {c.Type}"));
        string createTableSql = $"CREATE TABLE {request.TableName} (id INT AUTO_INCREMENT PRIMARY KEY, {columnDefinitions});";

        MySqlCommand cmd = new MySqlCommand(createTableSql, conn);
        cmd.ExecuteNonQuery();

        // Oluşturulan tabloyu Category tablosuna ekleyin
        string insertCategorySql = $"INSERT INTO categories (category_name) VALUES ('{request.TableName}');";
        MySqlCommand insertCategoryCmd = new MySqlCommand(insertCategorySql, conn);
        insertCategoryCmd.ExecuteNonQuery();

        Console.WriteLine("Table created successfully and added to categorys table");
        conn.Close();
        return Ok();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}


    public class CreateTableRequest
    {
        public string TableName { get; set; }
        public List<ColumnDefinition> ColumnDefinitions { get; set; }

        public class ColumnDefinition
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }
}