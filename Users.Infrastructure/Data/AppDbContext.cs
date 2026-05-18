using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(u => u.CreatedAt)
                    .IsRequired();

                entity.Property(u => u.UpdatedAt)
                    .IsRequired(false);

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("Idx_On_User_Email");

                /* SEEDING ADMIN DATA 
                    Email = admin@gmail.com
                    Password = admin@1111
                */
                //var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin@1111");
                //Console.WriteLine(hashedPassword);

                entity.HasData(new User
                {
                    Id = new Guid("540E371F-82C8-4D92-A6C4-A1C9364B4C30"),
                    FullName = "Admin User",
                    Email = "admin@gmail.com",
                    PasswordHash = "$2a$11$qQjHfNmmEDuj/Uq7nHoe8uoyE.NDP0iOAaI7XR6Cv2ikQoFvu.PQK",
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = null,
                });
            });


        }
    }
}
