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

    public class ComputersController : Controller
    {
        private readonly MySqlConnection _connection;

        public ComputersController()
        {
            _connection = new MySqlConnection("server=localhost;port=3306;database=vb_ecommerce;username=root;password=deneme");
        }

        [HttpGet]
        public async Task<IActionResult> GetComputers()
        {
            using (var connection = _connection)
            {
                connection.Open();

                var query = await connection.QueryAsync<Computer>("SELECT * FROM products_computer");

                var computers = query.AsList();

                return Ok(computers);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComputer(int id)
        {
            using (var connection = _connection)
            {
                connection.Open();

                var query = await connection.QuerySingleOrDefaultAsync<Computer>("SELECT * FROM products_computer WHERE Id = @Id", new { Id = id });

                if (query == null)
                {
                    return NotFound();
                }

                return Ok(query);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateComputer([FromBody] Computer computer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync("INSERT INTO products_computer (brand, model, processor, ram_capacity, storage_capacity, price, image_path) VALUES (@Brand, @Model, @Processor, @Ram_Capacity, @Storage_Capacity, @Price, @Image_Path)", computer);

                return CreatedAtAction(nameof(GetComputer), new { id = computer.Id }, computer);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComputer(int id, [FromBody] Computer computer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != computer.Id)
            {
                return BadRequest();
            }

            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync("UPDATE products_computer SET brand = @Brand, model = @Model, processor = @Processor, ram_capacity = @Ram_Capacity, storage_capacity = @Storage_Capacity, price = @Price, image_path = @Image_Path WHERE Id = @Id", computer);

                if (result == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComputer(int id)
        {
            using (var connection = _connection)
            {
                connection.Open();

                var result = await connection.ExecuteAsync("DELETE FROM products_computer WHERE Id = @Id", new { Id = id });

                if (result == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }
        
        [HttpGet("count")]
        public async Task<IActionResult> GetComputerCount()
        {
            using (var connection = _connection)
            {
                connection.Open();

                var query = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM products_computer");

                return Ok(query);
            }
        }
    }

    public class Computer
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Processor { get; set; }
        public int Ram_Capacity { get; set; }
        public int Storage_Capacity { get; set; }
        public double Price { get; set; }
        public string Image_Path { get; set; }
    }
}
