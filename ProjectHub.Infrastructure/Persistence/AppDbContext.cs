using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Users> Users => Set<Users>();
        public DbSet<Projects> Projects => Set<Projects>();
        public DbSet<Tables> Tables => Set<Tables>();
        public DbSet<Columns> Columns => Set<Columns>();
        public DbSet<Rows> Rows => Set<Rows>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasMany(u => u.Projects)
                .WithOne(p => p.Users)
                .HasForeignKey(p => p.User_id);

            modelBuilder.Entity<Projects>()
                .HasMany(p => p.Tables)
                .WithOne(t => t.Projects)
                .HasForeignKey(t => t.Project_id);

            modelBuilder.Entity<Tables>()
                .HasMany(t => t.Columns)
                .WithOne(c => c.Tables)
                .HasForeignKey(c => c.Table_id);

            modelBuilder.Entity<Tables>()
                .HasMany(t => t.Rows)
                .WithOne(r => r.Table)
                .HasForeignKey(r => r.Table_id);
        }
    }
}
