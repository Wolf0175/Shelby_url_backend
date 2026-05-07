using System.ComponentModel.DataAnnotations;

namespace UrlShortenerService.DTOs
{
    public class UrlRequestDto
    {
        [Required]
        [Url(ErrorMessage = "Please provide a valid URL (e.g., https://google.com)")]
        public string Url { get; set; } = string.Empty;
    }
}