using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
namespace VB_ecommerce_backend.Controllers;
[ApiController]
[Route("[controller]")]
public class CustomerController : Controller
{
    private readonly MySqlConnection _connection;
    private readonly ILogger<AuthController> _logger;

    public CustomerController( ILogger<AuthController> logger)
    {            
        _logger = logger;

        _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
    }
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        try
        {
            _connection.Open();

            string getUserQuery = "SELECT * FROM Users WHERE Username = @Username";
            var user = await _connection.QueryFirstOrDefaultAsync<UserForLogin>(getUserQuery, new { Username = username });

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound("Kullanıcı bulunamadı");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı sorgulanırken bir hata oluştu.");
            return StatusCode(500, "Bir hata oluştu.");
        }
    }
}