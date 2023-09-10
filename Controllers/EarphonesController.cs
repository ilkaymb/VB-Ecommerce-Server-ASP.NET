using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;

namespace VB_ecommerce_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class EarPhonesController : ControllerBase
    {
        private readonly MySqlConnection _connection;

        public EarPhonesController()
        {
            _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
        }

        [HttpGet]
        public async Task<IActionResult> GetEarPhones()
        {
            using (var connection = _connection)
            {
                connection.Open();

                var query = await connection.QueryAsync<EarPhone>("SELECT * FROM earphone");

                var earPhones = query.AsList();

                return Ok(earPhones);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEarPhone(int id)
        {
            using (var connection = _connection)
            {
                connection.Open();

                var query = await connection.QuerySingleOrDefaultAsync<EarPhone>("SELECT * FROM earphone WHERE Id = @Id", new { Id = id });

                if (query == null)
                {
                    return NotFound();
                }

                return Ok(query);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEarPhone([FromBody] EarPhone earPhone)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync("INSERT INTO earphone (brand, model, price, color, image_path,stock) VALUES (@Brand, @Model, @Price, @Color, @image_path,@Stock)", earPhone);

                return CreatedAtAction(nameof(GetEarPhone), new { id = earPhone.Id }, earPhone);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEarPhone(int id, [FromBody] EarPhone earPhone)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != earPhone.Id)
            {
                return BadRequest();
            }

            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync("UPDATE earphone SET brand = @Brand, model = @Model, price = @Price, color = @Color, image_path = @image_path,stock=@Stock WHERE Id = @Id", earPhone);

                if (result == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEarPhone(int id)
        {
            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync("DELETE FROM earphone WHERE Id = @Id", new { Id = id });

                if (result == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }
        
        [HttpGet("count")]
        public async Task<IActionResult> GetEarPhoneCount()
        {
            using (var connection = _connection)
            {
                connection.Open();

                var query = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM earphone");

                return Ok(query);
            }
        }
    }
}
public class EarPhone
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; }
    public string image_path { get; set; } // Resim dosya yolu sütunu
    public int Stock { get; set; }

}
