using System.ComponentModel.DataAnnotations;

namespace UrlShortenerService.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }

        [Required]
        [Url] // Validates that the input is an actual URL
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}