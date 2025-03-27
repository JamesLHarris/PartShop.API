using Microsoft.EntityFrameworkCore;
using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<DbCategory> Category { get; set; }

    }
}
