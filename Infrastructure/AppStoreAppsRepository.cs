using Microsoft.EntityFrameworkCore;
using StoreParsers.Domain;

namespace StoreParsers.Infrastructure
{
    public class AppStoreAppsRepository : DbContext
    {
        public DbSet<Application> Applications { get; set; }
        public DbSet<SearchRequest> SearchRequests { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=appstoredata.db");
        }
    }
}
