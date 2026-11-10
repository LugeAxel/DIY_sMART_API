using DIY_SMART_API.Models;
using DIY_SMART_API.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DIY_SMART_API.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginReq request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new LoginResponseWrapper
                {
                    Status = "error",
                    Message = "Email dan password harus diisi"
                });
            }

            var connectionString = _configuration.GetConnectionString("DIY_SMART_Connection");
            using var connection = new SqlConnection(connectionString);

            const string query = "SELECT id, email, password, nama, alamat, telepon FROM users WHERE email = @email";

            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@email", SqlDbType.VarChar).Value = request.Email;

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return Unauthorized(new LoginResponseWrapper
                    {
                        Status = "error",
                        Message = "Email atau password salah"
                    });
                }

                var userId = reader.GetInt32(0);
                var dbPassword = reader.GetString(2);
                var nama = reader.GetString(3);
                var alamat = reader.IsDBNull(4) ? "" : reader.GetString(4);
                var telepon = reader.IsDBNull(5) ? "" : reader.GetString(5);

                if (dbPassword != request.Password)
                {
                    return Unauthorized(new LoginResponseWrapper
                    {
                        Status = "error",
                        Message = "Email atau password salah"
                    });
                }

                var token = GenerateDummyToken(userId);

                return Ok(new LoginResponseWrapper
                {
                    Status = "success",
                    Message = "Login berhasil",
                    Authorization = new Authorization
                    {
                        Token = token,
                        Type = "Bearer",
                        ExpiresIn = 3600
                    },
                    Data = new LoginResponseData
                    {
                        Id = userId,
                        Name = nama,
                        Email = request.Email,
                        Phone = telepon,
                        Address = alamat,
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new LoginResponseWrapper
                {
                    Status = "error",
                    Message = "Terjadi kesalahan pada database: " + ex.Message
                });
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterReq request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || 
                string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Phone))
            {
                return BadRequest(new BaseResponse<object>
                {
                    Status = "error",
                    Message = "Semua field harus diisi"
                });
            }

            if (request.Password != request.PasswordConfirmation)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Status = "error",
                    Message = "Password dan konfirmasi password tidak cocok"
                });
            }

            var connectionString = _configuration.GetConnectionString("DIY_SMART_Connection");
            using var connection = new SqlConnection(connectionString);

            const string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @email";
            using var checkCommand = new SqlCommand(checkQuery, connection);
            checkCommand.Parameters.Add("@email", SqlDbType.VarChar).Value = request.Email;

            try
            {
                connection.Open();
                var emailExists = (int)checkCommand.ExecuteScalar() > 0;
                if (emailExists)
                {
                    return Conflict(new BaseResponse<object>
                    {
                        Status = "error",
                        Message = "Email sudah terdaftar"
                    });
                }

                const string insertQuery = "INSERT INTO users (tipe_user, nama, alamat, telepon, email, password) VALUES (@tipe_user, @nama, @alamat, @telepon, @email, @password)";
                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.Add("@tipe_user", SqlDbType.VarChar).Value = "kasir";
                insertCommand.Parameters.Add("@nama", SqlDbType.VarChar).Value = request.Name;
                insertCommand.Parameters.Add("@alamat", SqlDbType.VarChar).Value = request.Address ?? "";
                insertCommand.Parameters.Add("@telepon", SqlDbType.VarChar).Value = request.Phone;
                insertCommand.Parameters.Add("@email", SqlDbType.VarChar).Value = request.Email;
                insertCommand.Parameters.Add("@password", SqlDbType.VarChar).Value = request.Password;

                insertCommand.ExecuteNonQuery();

                return Ok(new BaseResponse<object>
                {
                    Status = "success",
                    Message = "Registrasi berhasil"
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new BaseResponse<object>
                {
                    Status = "error",
                    Message = "Terjadi kesalahan: " + ex.Message
                });
            }
        }

        private string GenerateDummyToken(int userId)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId.ToString() + DateTime.UtcNow.Ticks));
        }
    }
}
