

using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher>().HasData(
                new Teacher { Id = 1, Name = "John Doe" },
                new Teacher { Id = 2, Name = "Jane Smith" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Login = "admin",
                    Password = "admin",
                    Role = "admin",
                    TeacherId = null
                },
                new User
                {
                    Id = 2,
                    Login = "teacher1",
                    Password = "123",
                    Role = "teacher",
                    TeacherId = 1
                },
                new User
                {
                    Id = 3,
                    Login = "teacher2",
                    Password = "1234",
                    Role = "teacher",
                    TeacherId = 2
                }
            );
        }
    }
}
