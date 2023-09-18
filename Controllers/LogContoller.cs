using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using MySql.Data.MySqlClient;
using MySql.Data;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;
        private readonly MySqlConnection _connection;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
            _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");

        }


        [HttpGet]
        public IActionResult GetLogs()
        {
            try
            {
                _connection.Open();
                var sqlCommand = new MySqlCommand("SELECT * FROM Logs", _connection);
                var logs = new List<Log>();

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new Log
                        {
                            Id = reader.GetInt32("Id"),
                            Timestamp = reader.GetDateTime("Timestamp"),
                            UserType = reader.GetString("UserType"),
                            UserName = reader.GetString("UserName"),
                            Action = reader.GetString("Action"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            ErrorDetails = reader.IsDBNull(reader.GetOrdinal("ErrorDetails")) ? null : reader.GetString("ErrorDetails")
                        };

                        logs.Add(log);
                    }
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sunucu hatası: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetLog(int id)
        {
            try
            {
                _connection.Open();
                var sqlCommand = new MySqlCommand("SELECT * FROM Logs WHERE Id = @id", _connection);
                sqlCommand.Parameters.AddWithValue("@id", id);

                using (var reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var log = new Log
                        {
                            Id = reader.GetInt32("Id"),
                            Timestamp = reader.GetDateTime("Timestamp"),
                            UserType = reader.GetString("UserType"),
                            UserName = reader.GetString("UserName"),
                            Action = reader.GetString("Action"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            ErrorDetails = reader.IsDBNull(reader.GetOrdinal("ErrorDetails")) ? null : reader.GetString("ErrorDetails")
                        };

                        return Ok(log);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sunucu hatası: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }
        }

        [HttpPost]
        public IActionResult AddLog([FromBody] Log log)
        {
            if (log == null)
            {
                return BadRequest("Log bilgileri eksik veya hatalı.");
            }

            try
            {
                _connection.Open();
                var sqlCommand = new MySqlCommand(
                    "INSERT INTO Logs (UserType, UserName, Action, Description, ErrorDetails) " +
                    "VALUES ( @userType, @userName, @action, @description, @errorDetails)", _connection);

                sqlCommand.Parameters.AddWithValue("@userType", log.UserType);
                sqlCommand.Parameters.AddWithValue("@userName", log.UserName);
                sqlCommand.Parameters.AddWithValue("@action", log.Action);
                sqlCommand.Parameters.AddWithValue("@description", log.Description);
                sqlCommand.Parameters.AddWithValue("@errorDetails", log.ErrorDetails);

                sqlCommand.ExecuteNonQuery();
                return Ok("Log başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sunucu hatası: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateLog(int id, [FromBody] Log updatedLog)
        {
            if (updatedLog == null || id != updatedLog.Id)
            {
                return BadRequest("Log bilgileri eksik veya hatalı.");
            }

            try
            {
                _connection.Open();
                var sqlCommand = new MySqlCommand(
                    "UPDATE Logs " +
                    "SET UserType = @userType, UserName = @userName, Action = @action, " +
                    "Description = @description, ErrorDetails = @errorDetails " +
                    "WHERE Id = @id", _connection);

                sqlCommand.Parameters.AddWithValue("@userType", updatedLog.UserType);
                sqlCommand.Parameters.AddWithValue("@userName", updatedLog.UserName);
                sqlCommand.Parameters.AddWithValue("@action", updatedLog.Action);
                sqlCommand.Parameters.AddWithValue("@description", updatedLog.Description);
                sqlCommand.Parameters.AddWithValue("@errorDetails", updatedLog.ErrorDetails );
                sqlCommand.Parameters.AddWithValue("@id", id);

                var rowsAffected = sqlCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Log başarıyla güncellendi.");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sunucu hatası: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteLog(int id)
        {
            try
            {
                _connection.Open();
                var sqlCommand = new MySqlCommand("DELETE FROM Logs WHERE Id = @id", _connection);
                sqlCommand.Parameters.AddWithValue("@id", id);

                var rowsAffected = sqlCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Log başarıyla silindi.");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sunucu hatası: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }
        }

 

        
        
    }
    public class Log
    {
        [Key]
        public int Id { get; set; } // Otomatik artan bir ID alanı

        [Required]
        public DateTime Timestamp { get; set; } // Logun kaydedildiği tarih ve saat
        
        [Required]
        public string UserType { get; set; } // İşlemi gerçekleştiren kullanıcının adı

        [Required]
        public string UserName { get; set; } // İşlemi gerçekleştiren kullanıcının adı

        [Required]
        public string Action { get; set; } // İşlem türü veya eylemi (örneğin, "Giriş Yapma")

        public string Description { get; set; } // İşlem hakkında daha fazla açıklama

        public string ErrorDetails { get; set; } // Hata detayları (eğer bir hata oluştuysa)

        // Diğer log alanlarını buraya ekleyebilirsiniz
    }
}