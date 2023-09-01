using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace VB_ecommerce_backend.Controllers
{
    [ApiController]    
    [Route("[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly MySqlConnection _connection;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");


        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegistration userForRegistration)
        {
            try
            {
                // Şifreyi hashleme işlemi
                string hashedPassword = HashPassword(userForRegistration.Password);

                // Aynı kullanıcı adına sahip başka bir kullanıcının varlığını kontrol etme
                _connection.Open();

                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                var existingUserCount = await _connection.ExecuteScalarAsync<int>(checkUserQuery, new { Username = userForRegistration.Username });

                if (existingUserCount > 0)
                {
                    return StatusCode(200, new { message = "Kullanıcı adı zaten kullanılıyor" });
                }

                string insertQuery = "INSERT INTO Users (Username, Password, RoleId) VALUES (@Username, @Password, @RoleId)";

                UserForLogin user = new UserForLogin();
                user.Username = userForRegistration.Username;
                user.Password = hashedPassword;
                user.RoleId = GetDefaultRoleId(); // Varsayılan rolü almak için kullanılan metot

                var result = await _connection.ExecuteAsync(insertQuery, user);

                return StatusCode(200, new { message = "Kullanıcı başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kaydı sırasında bir hata oluştu.");
                return StatusCode(500, new { message = "Bir hata oluştu." });
            }
        }

        private int GetDefaultRoleId()
        {
            // Varsayılan rolünüzün veritabanındaki ID'sini almak için gerekli kodu burada ekleyin.
            // Örneğin, "customer" rolünün ID'sini belirli bir sorgu veya mantıksal işlemle alabilirsiniz.
            // Daha sonra bu ID'yi yukarıdaki kod parçasında kullanın.
            // Bu, veritabanınıza ve veri modelinize bağlı olarak değişebilir.
            // Bu örnek için varsayılan rolü bulmak için bir metot tanımlanmıştır, gerçek uygulamanızda gerektiği gibi uyarlayabilirsiniz.
            return 1; // Örneğin, 1 varsayılan "customer" rolü ID'si olarak kabul ediliyor.
        }
  [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] UserForLogin userForLogin)
{
    try
    {
        await _connection.OpenAsync();

        string getUserQuery = "SELECT * FROM Users WHERE Username = @Username";
        var user = await _connection.QueryFirstOrDefaultAsync<UserForLogin>(getUserQuery, new { Username = userForLogin.Username });

        if (user == null)
        {
            return BadRequest("Kullanıcı bulunamadı.");
        }
        string hashedPassword = HashPassword(userForLogin.Password);

        // Kullanıcının girdiği parolayı ve veritabanındaki hash'i karşılaştırma
        if (hashedPassword != user.Password)
        {
            return Unauthorized("Hatalı şifre.");
        }

        // Başarılı giriş işlemi
        UserLoginResponse userLoginResponse = new UserLoginResponse();
        userLoginResponse.Username = user.Username;
        userLoginResponse.RoleId = user.RoleId;
        userLoginResponse.ID = user.ID;

        // JWT token oluşturma
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("buraya-en-az-256-bit-uzunlugunda-bir-anahtar-yaz");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Username), // Kullanıcı kimliği
                new Claim("role", user.RoleId.ToString()), // Kullanıcı rolü (admin veya müşteri)
                // Diğer bilgileri de ekleyebilirsiniz
            }),
            Expires = DateTime.UtcNow.AddHours(1), // Token'ın süresi
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Kullanıcıya token ile birlikte cevap dön
        return Ok(new { userLoginResponse, token = tokenString });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Giriş sırasında bir hata oluştu.");
        return StatusCode(500, ex);
    }
}
 

        private string HashPassword(string password)
        {
            // Güvenli bir şifreleme işlemi için uygun bir kütüphane kullanmalısınız.
            // Örneğin, ASP.NET Core Identity veya BCrypt gibi kütüphaneler kullanabilirsiniz.
            // Aşağıda basit bir SHA-256 örneği bulunmaktadır, güvenli değildir, sadece referans içindir.
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }

    public class UserForRegistration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }


        // Ek alanlar eklenebilir (örneğin, e-posta, ad-soyad vb.)
    }

    public class UserForLogin
    {
        public int ID { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }

    }
    
    public class UserLoginResponse
    {
        public string Username { get; set; }
        public int ID { get; set; }

        public int RoleId { get; set; }
        // Diğer kullanıcı bilgileri buraya ekleyebilirsiniz.
    }

    
    
}