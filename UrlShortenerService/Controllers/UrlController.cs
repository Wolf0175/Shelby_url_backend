using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data;
using UrlShortenerService.DTOs;
using UrlShortenerService.Models;

namespace UrlShortenerService.Controllers
{
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UrlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /api/url/shorten
        [HttpPost("api/url/shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Check if we already shortened this URL to save database space
            var existingUrl = await _context.UrlMappings.FirstOrDefaultAsync(u => u.OriginalUrl == request.Url);
            if (existingUrl != null)
            {
                var existingShortUrl = $"{Request.Scheme}://{Request.Host}/{existingUrl.ShortCode}";
                return Ok(new { shortUrl = existingShortUrl });
            }

            // 2. Generate a random 6-character short code
            var shortCode = GenerateShortCode(6);

            // 3. Save to database
            var urlMapping = new UrlMapping
            {
                OriginalUrl = request.Url,
                ShortCode = shortCode
            };

            _context.UrlMappings.Add(urlMapping);
            await _context.SaveChangesAsync();

            // 4. Return the new shortened URL
            var shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(new { shortUrl = shortUrl });
        }

        // GET: /{code}
        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var mapping = await _context.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == code);

            if (mapping == null)
            {
                return NotFound("Short URL not found.");
            }

            // Perform the 302 Redirect to the original long URL
            return Redirect(mapping.OriginalUrl);
        }

        // Helper method to generate random string
        private string GenerateShortCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}