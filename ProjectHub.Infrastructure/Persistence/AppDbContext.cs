using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSet สำหรับ Entity ทั้งหมด
        public DbSet<Users> Users { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Tables> Tables { get; set; }
        public DbSet<Columns> Columns { get; set; }
        public DbSet<Rows> Rows { get; set; }
        public DbSet<Relationships> Relationships { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // เรียก base ก่อนเสมอ

            // --- 1. บอก EF Core ให้ใช้ Type "jsonb" สำหรับคอลัมน์ 'Data' ---
            modelBuilder.Entity<Rows>().Property(r => r.Data).HasColumnType("jsonb");

            modelBuilder.Entity<Users>()
                  .HasKey(u => u.User_id);
            modelBuilder.Entity<Users>().Property(u => u.User_id)
                .ValueGeneratedOnAdd(); // <-- [ADD] บอกว่านี่คือ Auto-increment

            modelBuilder.Entity<Projects>()
                .HasKey(p => p.Project_id);
            modelBuilder.Entity<Projects>().Property(p => p.Project_id)
                .ValueGeneratedOnAdd(); // <-- [ADD] บอกว่านี่คือ Auto-increment

            modelBuilder.Entity<Tables>()
                .HasKey(t => t.Table_id);
            modelBuilder.Entity<Tables>().Property(t => t.Table_id)
                .ValueGeneratedOnAdd(); // <-- [ADD] บอกว่านี่คือ Auto-increment

            modelBuilder.Entity<Columns>()
                .HasKey(c => c.Column_id);
            modelBuilder.Entity<Columns>().Property(c => c.Column_id)
                .ValueGeneratedOnAdd(); // <-- [ADD] บอกว่านี่คือ Auto-increment

            modelBuilder.Entity<Rows>()
                .HasKey(r => r.Row_id);
            modelBuilder.Entity<Rows>().Property(r => r.Row_id)
                .ValueGeneratedOnAdd(); // <-- [ADD] บอกว่านี่คือ Auto-increment

            modelBuilder.Entity<Relationships>()
                .HasKey(r => r.RelationshipId);
            modelBuilder.Entity<Relationships>().Property(r => r.RelationshipId)
                .ValueGeneratedOnAdd(); // <-- [ADD] บอกว่านี่คือ Auto-increment
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

            // --- 3. Cascade Delete สำหรับ Relationships (ฉบับแก้ไข) ---

            // [FIX 1]
            // ถ้า Table (ต้นทาง) ถูกลบ -> *ห้าม* Cascade (NoAction)
            // (เพราะเราจะปล่อยให้ "Column" เป็นคนสั่งลบ Relationship เอง)
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.PrimaryTable)
                .WithMany()
                .HasForeignKey(r => r.PrimaryTableId)
                .OnDelete(DeleteBehavior.NoAction); // <-- [FIX] แก้เป็น NoAction

            // [FIX 2]
            // ถ้า Table (ปลายทาง) ถูกลบ -> *ห้าม* Cascade (NoAction)
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.ForeignTable)
                .WithMany()
                .HasForeignKey(r => r.ForeignTableId)
                .OnDelete(DeleteBehavior.NoAction); // <-- (อันนี้ NoAction ถูกแล้ว)

            // [FIX 3]
            // ถ้า Column (ต้นทาง) ถูกลบ -> ให้ลบ "กฎ" ความสัมพันธ์นี้ทิ้ง
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.PrimaryColumn)
                .WithMany()
                .HasForeignKey(r => r.PrimaryColumnId)
                .OnDelete(DeleteBehavior.Cascade); // <-- (อันนี้ Cascade ถูกแล้ว)

            // [FIX 4]
            // ถ้า Column (ปลายทาง) ถูกลบ -> ให้ลบ "กฎ" ความสัมพันธ์นี้ทิ้ง
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.ForeignColumn)
                .WithMany()
                .HasForeignKey(r => r.ForeignColumnId)
                .OnDelete(DeleteBehavior.Cascade); // <-- [FIX] แก้เป็น Cascade
        }
    }
}