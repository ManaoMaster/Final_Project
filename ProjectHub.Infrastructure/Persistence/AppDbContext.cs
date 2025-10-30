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
        public DbSet<Relationships> Relationships { get; set; } // <-- เพิ่ม DbSet ใหม่

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // เรียก base ก่อนเสมอ

            // --- 1. บอก EF Core ให้ใช้ Type "jsonb" สำหรับคอลัมน์ 'Data' ---
            modelBuilder.Entity<Rows>().Property(r => r.Data).HasColumnType("jsonb"); // (ถ้าใช้ SQLite ให้ Comment บรรทัดนี้)

            // --- 2. การตั้งค่า Cascade Delete (ของเดิม) ---

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

            // --- 3. Cascade Delete สำหรับ Relationships (ส่วนที่เพิ่มใหม่) ---

            // ถ้า Table (ต้นทาง) ถูกลบ -> ให้ลบ "กฎ" ความสัมพันธ์นี้ทิ้ง
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.PrimaryTable)
                .WithMany() // Table 1 Table มีได้หลาย Relationships (แต่ Tables Entity ไม่มี ICollection<Relationships>)
                .HasForeignKey(r => r.PrimaryTableId)
                .OnDelete(DeleteBehavior.Cascade); // <-- สั่งลบ "กฎ" ทิ้ง

            // ถ้า Table (ปลายทาง) ถูกลบ -> *ห้าม* Cascade (ป้องกัน Multiple Cascade Paths)
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.ForeignTable)
                .WithMany()
                .HasForeignKey(r => r.ForeignTableId)
                .OnDelete(DeleteBehavior.NoAction); // <-- ตั้งเป็น NoAction

            // ถ้า Column (ต้นทาง) ถูกลบ -> ให้ลบ "กฎ" ความสัมพันธ์นี้ทิ้ง
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.PrimaryColumn)
                .WithMany()
                .HasForeignKey(r => r.PrimaryColumnId)
                .OnDelete(DeleteBehavior.Cascade); // <-- สั่งลบ "กฎ" ทิ้ง

            // ถ้า Column (ปลายทาง) ถูกลบ -> *ห้าม* Cascade (ป้องกัน Multiple Cascade Paths)
            modelBuilder
                .Entity<Relationships>()
                .HasOne(r => r.ForeignColumn)
                .WithMany()
                .HasForeignKey(r => r.ForeignColumnId)
                .OnDelete(DeleteBehavior.NoAction); // <-- ตั้งเป็น NoAction
        }
    }
}
