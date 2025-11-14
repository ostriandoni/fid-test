using Fujitsu.Models;
using Microsoft.EntityFrameworkCore;

namespace Fujitsu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Supplier> Suppliers { get; set; }
    }
}