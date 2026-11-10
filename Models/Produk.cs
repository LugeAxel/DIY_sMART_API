using System.Text.Json.Serialization;

namespace DIY_SMART_API.Models
{
    public class Produk
    {
        public int Id { get; set; }
        public string KodeBarang { get; set; } = string.Empty;
        public string NamaBarang { get; set; } = string.Empty;
        public DateTime ExpiredDate { get; set; }
        public long JumlahBarang { get; set; }
        public string Satuan { get; set; } = string.Empty;
        public long HargaSatuan { get; set; }
    }

    public class ProductItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public string Price { get; set; } = string.Empty;

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public string Rating { get; set; } = "4.5";

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class ProductRes
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "success";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<ProductItemDto>? Data { get; set; }
    }

    public class ProductDetailRes
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "success";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public ProductItemDto? Data { get; set; }
    }
}
