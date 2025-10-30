// ใน ProjectHub.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Tables> Tables { get; set; }
        public DbSet<Columns> Columns { get; set; }
        public DbSet<Rows> Rows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // เรียก base ก่อน

            // --- 1. บอก EF Core ให้ใช้ Type "jsonb" สำหรับคอลัมน์ 'Data' ---
            modelBuilder.Entity<Rows>().Property(r => r.Data).HasColumnType("jsonb"); // <-- นี่คือหัวใจของ "ระดับ 2"

            // --- 2. การตั้งค่า Cascade Delete (เหมือนเดิม) ---

            // ลบ User -> ลบ Projects
            modelBuilder
                .Entity<Users>()
                .HasMany(u => u.Projects)
                .WithOne(p => p.Users)
                .HasForeignKey(p => p.User_id)
                .OnDelete(DeleteBehavior.Cascade);

            // ลบ Project -> ลบ Tables
            modelBuilder
                .Entity<Projects>()
                .HasMany(p => p.Tables)
                .WithOne(t => t.Projects)
                .HasForeignKey(t => t.Project_id)
                .OnDelete(DeleteBehavior.Cascade);

            // ลบ Table -> ลบ Columns
            modelBuilder
                .Entity<Tables>()
                .HasMany(t => t.Columns)
                .WithOne(c => c.Tables)
                .HasForeignKey(c => c.Table_id)
                .OnDelete(DeleteBehavior.Cascade);

            // ลบ Table -> ลบ Rows
            modelBuilder
                .Entity<Tables>()
                .HasMany(t => t.Rows)
                .WithOne(r => r.Table)
                .HasForeignKey(r => r.Table_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
