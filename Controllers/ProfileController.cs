using DIY_SMART_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DIY_SMART_API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProfileController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new BaseResponse<ProfileData>
                {
                Status = "error",
                    Message = "Token tidak ditemukan"
                });
            }

            var connectionString = _configuration.GetConnectionString("DIY_SMART_Connection");
            using var connection = new SqlConnection(connectionString);

            const string query = "SELECT TOP 1 id, nama, email, telepon, alamat FROM users ORDER BY id DESC";

            using var command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return NotFound(new BaseResponse<ProfileData>
                    {
                    Status = "error",
                        Message = "User tidak ditemukan"
                    });
                }

                var profileData = new ProfileData
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Address = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    CreatedAt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Ok(new BaseResponse<ProfileData>
                {
                    Status = "success",
                    Message = "Profil berhasil diambil",
                    Data = profileData
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new BaseResponse<ProfileData>
                {
                Status = "error",
                    Message = "Terjadi kesalahan: " + ex.Message
                });
            }
        }
    }
}
