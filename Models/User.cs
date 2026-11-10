namespace DIY_SMART_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string TipeUser { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Alamat { get; set; } = string.Empty;
        public string Telepon { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
