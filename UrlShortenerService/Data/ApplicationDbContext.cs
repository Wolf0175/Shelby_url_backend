using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Models;

namespace UrlShortenerService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UrlMapping> UrlMappings { get; set; }

        // ADD THIS: Tells the database to create a Users table
        public DbSet<User> Users { get; set; }
    }
}