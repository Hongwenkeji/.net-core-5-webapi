using Microsoft.EntityFrameworkCore;

namespace webapi.Models
{
    public class AppletContext : DbContext
    {
        public AppletContext(DbContextOptions<AppletContext> options)
            : base(options)
        {
        }
        public DbSet<Admin> Admin { get; set; }
    }
}