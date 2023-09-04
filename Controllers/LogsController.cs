using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
namespace VB_ecommerce_backend.Controllers;
[ApiController]    
[Route("[controller]")]
public class LogsController : Controller
{
    private readonly MySqlConnection _connection;
    private readonly ILogger _logger;

    public LogsController(ILogger logger)
    {
        _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
        _logger = logger;

    }
    // Tüm logları getiren endpoint
   [HttpGet]
    public IActionResult Get()
    {
        string hostName = Dns.GetHostName();
        IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
        _logger.LogInformation("Bu bir bilgi mesajıdır.");

        return Ok();
    }


}