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
            base.OnModelCreating(modelBuilder); // เรียก base ก่อนเสมอ

            // กำหนด Cascade Delete

            // 1. ลบ User -> ลบ Projects ที่เกี่ยวข้อง (เพิ่มเข้ามาใหม่)
            modelBuilder
                .Entity<Users>()
                .HasMany(u => u.Projects) // User มีหลาย Projects
                .WithOne(p => p.Users) // Project มี User เดียว
                .HasForeignKey(p => p.User_id) // Foreign Key คือ User_id ใน Projects
                .OnDelete(DeleteBehavior.Cascade); // <-- สั่งให้ Cascade Delete (ลบ User จะลบ Project ด้วย)
            // หรือใช้ .OnDelete(DeleteBehavior.Restrict) ถ้าไม่ต้องการให้ลบ User ที่ยังมี Project อยู่

            // 2. ลบ Project -> ลบ Tables ที่เกี่ยวข้อง
            modelBuilder
                .Entity<Projects>()
                .HasMany(p => p.Tables) // Project มีหลาย Tables
                .WithOne(t => t.Projects) // Table มี Project เดียว
                .HasForeignKey(t => t.Project_id) // Foreign Key คือ Project_id
                .OnDelete(DeleteBehavior.Cascade); // <-- สั่งให้ Cascade Delete

            // 3. ลบ Table -> ลบ Columns ที่เกี่ยวข้อง
            modelBuilder
                .Entity<Tables>()
                .HasMany(t => t.Columns) // Table มีหลาย Columns
                .WithOne(c => c.Tables) // Column มี Table เดียว
                .HasForeignKey(c => c.Table_id) // Foreign Key คือ Table_id
                .OnDelete(DeleteBehavior.Cascade); // <-- สั่งให้ Cascade Delete

            // 4. ลบ Table -> ลบ Rows ที่เกี่ยวข้อง
            modelBuilder
                .Entity<Tables>()
                .HasMany(t => t.Rows) // Table มีหลาย Rows
                .WithOne(r => r.Table) // Row มี Table เดียว
                .HasForeignKey(r => r.Table_id) // Foreign Key คือ Table_id
                .OnDelete(DeleteBehavior.Cascade); // <-- สั่งให้ Cascade Delete

            // คุณอาจจะต้องกำหนด Relationship อื่นๆ เพิ่มเติมถ้าจำเป็น
        }
    }
}
