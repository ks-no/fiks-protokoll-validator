using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KS.FiksProtokollValidator.WebAPI.Data
{
    public class FiksIOMessageDBContext : DbContext
    {
        public FiksIOMessageDBContext(DbContextOptions<FiksIOMessageDBContext> options)
            : base(options)
        {
        }

        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }
        public DbSet<FiksResponseTest> FiksResponseTest { get; set; }
        public DbSet<FiksRequest> FiksRequest { get; set; }
        public DbSet<FiksResponse> FiksResponse { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestCase>().HasIndex(t => t.TestId).IsUnique();
        }
    }
    
    public class FiksIOMessageDBContextFactory : IDesignTimeDbContextFactory<FiksIOMessageDBContext>
    {
        public FiksIOMessageDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FiksIOMessageDBContext>();
            optionsBuilder.UseSqlServer("your connection string");

            return new FiksIOMessageDBContext(optionsBuilder.Options);
        }
    }
}
