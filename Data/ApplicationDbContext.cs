using Microsoft.EntityFrameworkCore;
using Bank_Slip_Scanner_App.Models;

namespace Bank_Slip_Scanner_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        private object e;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ;
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Actif).HasDefaultValue(true);
                entity.Property(u => u.CompteVerrouille).HasDefaultValue(false);
                entity.Property(u => u.TentativesConnexion).HasDefaultValue(0);
                entity.Property(u => u.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");

            });
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasIndex(s => s.TokenSession);
                entity.HasIndex(s => s.RefreshToken);
                entity.HasOne(s => s.Users)
                .WithMany()
                .HasForeignKey(s => s.IdUsers)
                .OnDelete(DeleteBehavior.Cascade);

            });
        }

    }
}


