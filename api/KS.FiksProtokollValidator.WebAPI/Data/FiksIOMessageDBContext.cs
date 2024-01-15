using System.IO;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<FiksIOMessageDBContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FiksIOMessageDBContext(optionsBuilder.Options);
        }
    }
}
