using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

            // 1. Generate a random 6-character short code
            var shortCode = GenerateShortCode(6);

            // 2. Prepare the new mapping
            var urlMapping = new UrlMapping
            {
                OriginalUrl = request.Url,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                // --- NEW CODE: Automatically set expiration to 30 days from now ---
                ExpiredDate = DateTime.UtcNow.AddDays(30)
            };

            // 3. Attach the User ID if they are logged in with a valid JWT Token
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                urlMapping.UserId = loggedInUserId;
            }

            // 4. Save to Neon PostgreSQL database
            _context.UrlMappings.Add(urlMapping);
            await _context.SaveChangesAsync();

            // 5. Return the new shortened URL back to Vue.js
            var shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(new { shortUrl = shortUrl });
        }

        // GET: /{code}
        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            // Find the URL mapping in the database
            var mapping = await _context.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == code);

            if (mapping == null)
            {
                return NotFound("Short URL not found.");
            }


            // --- NEW CODE: Check if the link has expired ---
            if (mapping.ExpiredDate.HasValue && mapping.ExpiredDate.Value < DateTime.UtcNow)
            {
                return BadRequest("This shortened URL has expired.");
            }
            // ----------------------------------------------


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