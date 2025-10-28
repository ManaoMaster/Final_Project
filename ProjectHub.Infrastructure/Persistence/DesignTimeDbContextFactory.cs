using System.IO; // เพิ่ม Using นี้
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // เพิ่ม Using นี้

namespace ProjectHub.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // --- แก้ไข: เปลี่ยน Logic การค้นหา Connection String ---

        // 1. กำหนดค่า Path ไปยัง API Project (ที่ appsettings.json อาศัยอยู่)
        // Path นี้คือการเดินย้อนกลับจาก Infrastructure -> FINAL_PROJECT -> ProjectHub.API
        string apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "../ProjectHub.infrastructure"
        );

        // 2. อ่าน appsettings.json จาก API Project
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        // 3. ดึง Connection String (เหมือนใน Program.cs)
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            // *** ใช้ Fallback Path เดียวกันกับ Program.cs ***
            ?? "Data Source=../ProjectHub.db";

        // 4. สร้าง Options Builder
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
