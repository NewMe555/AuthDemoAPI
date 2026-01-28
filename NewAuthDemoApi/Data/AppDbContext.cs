using AuthDemoApi.Models;
using Microsoft.EntityFrameworkCore;
namespace AuthDemoApi.Data
{
    public class AppDbContext : DbContext
     {
        //“This allows dependency injection to provide the database configuration when the context is created.”
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }//Ye constructor EF Core ko database connection info provide karta hai.
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var password = BCrypt.Net.BCrypt.HashPassword("12345");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "vaibhavi",
                    PasswordHash = password,
                    //RefreshToken = "",
                    //RefreshTokenExpiryTime = DateTime.MinValue,
                    LastLoginAt=null
                }
                );

        }
        
     }
}
