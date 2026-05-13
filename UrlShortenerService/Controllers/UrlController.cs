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

        // GET: /api/url/my-links
        [HttpGet("api/url/my-links")]
        public async Task<IActionResult> GetMyLinks()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { message = "You must be logged in to view your links." });
            }

            // 1. Fetch data from DB first (Without confusing the database with Request properties)
            var myLinksData = await _context.UrlMappings
                .Where(u => u.UserId == loggedInUserId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // 2. Format the URLs safely in server memory
            var myLinks = myLinksData.Select(u => new
            {
                u.Id,
                u.OriginalUrl,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{u.ShortCode}",
                u.CreatedAt,
                u.ExpiredDate
            });

            return Ok(myLinks);
        }

        // POST: /api/url/shorten
        [HttpPost("api/url/shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var shortCode = GenerateShortCode(6);

            var urlMapping = new UrlMapping
            {
                OriginalUrl = request.Url,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ExpiredDate = DateTime.UtcNow.AddDays(30)
            };

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                urlMapping.UserId = loggedInUserId;
            }

            _context.UrlMappings.Add(urlMapping);
            await _context.SaveChangesAsync();

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

            if (mapping.ExpiredDate.HasValue && mapping.ExpiredDate.Value < DateTime.UtcNow)
            {
                return BadRequest("This shortened URL has expired.");
            }

            return Redirect(mapping.OriginalUrl);
        }

        private string GenerateShortCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}