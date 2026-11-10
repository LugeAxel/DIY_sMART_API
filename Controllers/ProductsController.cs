using DIY_SMART_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DIY_SMART_API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string? search = null)
        {
            var connectionString = _configuration.GetConnectionString("DIY_SMART_Connection");
            var products = new List<ProductItemDto>();
            using var connection = new SqlConnection(connectionString);

            string query = "SELECT id, kode_barang, nama_barang, expired_date, jumlah_barang, satuan, harga_satuan FROM produk";
            if (!string.IsNullOrEmpty(search))
            {
                query += " WHERE nama_barang LIKE @search OR kode_barang LIKE @search";
            }
            query += " ORDER BY id";

            using var command = new SqlCommand(query, connection);
            if (!string.IsNullOrEmpty(search))
            {
                command.Parameters.Add("@search", SqlDbType.VarChar).Value = $"%{search}%";
            }

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var nama = reader.GetString(2);
                    var harga = reader.GetInt64(6);

                    products.Add(new ProductItemDto
                    {
                        Id = id,
                        Name = nama,
                        Price = harga.ToString(),
                        ImageUrl = GetProductImage(nama),
                        Rating = "4.8",
                        CreatedAt = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return Ok(new ProductRes
                {
                    Status = "success",
                    Message = "Berhasil memuat daftar produk",
                    Data = products
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ProductRes
                {
                    Status = "error",
                    Message = "Gagal memuat produk: " + ex.Message,
                    Data = null
                });
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetProductById(int id)
        {
            var connectionString = _configuration.GetConnectionString("DIY_SMART_Connection");
            using var connection = new SqlConnection(connectionString);

            const string query = "SELECT id, kode_barang, nama_barang, expired_date, jumlah_barang, satuan, harga_satuan FROM produk WHERE id = @id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return NotFound(new ProductDetailRes
                    {
                        Status = "error",
                        Message = $"Produk dengan ID {id} tidak ditemukan"
                    });
                }

                var nama = reader.GetString(2);
                var harga = reader.GetInt64(6);

                var product = new ProductItemDto
                {
                    Id = reader.GetInt32(0),
                    Name = nama,
                    Price = harga.ToString(),
                    ImageUrl = GetProductImage(nama),
                    Rating = "4.8",
                    CreatedAt = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Ok(new ProductDetailRes
                {
                    Status = "success",
                    Message = "Produk ditemukan",
                    Data = product
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ProductDetailRes
                {
                    Status = "error",
                    Message = "Error: " + ex.Message
                });
            }
        }

        private static string GetProductImage(string name)
        {
            var lower = name.ToLower();
            if (lower.Contains("fanta")) return "https://images.unsplash.com/photo-1624517452488-04869289c4ca?w=300";
            if (lower.Contains("milo")) return "https://images.unsplash.com/photo-1541658016709-82535e94bc69?w=300";
            if (lower.Contains("sprite")) return "https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=300";
            if (lower.Contains("coca")) return "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=300";
            return "https://images.unsplash.com/photo-1584473457406-6240486418e9?w=300";
        }
    }
}
