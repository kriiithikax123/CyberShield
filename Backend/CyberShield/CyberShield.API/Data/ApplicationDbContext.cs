using Microsoft.EntityFrameworkCore;
using CyberShield.API.Models;

namespace CyberShield.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ScanResult> ScanResults { get; set; }
    }
}
