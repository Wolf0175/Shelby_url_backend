namespace UrlShortenerService.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- NEW COLUMNS ---

        // Nullable DateTime because some links might never expire
        public DateTime? ExpiredDate { get; set; }

        // Nullable UserId so anonymous guests can still create links if you want
        public int? UserId { get; set; }

        // Navigation property pointing to the User model
        public User? User { get; set; }
    }
}