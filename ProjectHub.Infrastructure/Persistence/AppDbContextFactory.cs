using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; 
using System;
using System.IO;

namespace ProjectHub.Infrastructure.Persistence
{
    // This factory is used by dotnet-ef tools to create the DbContext instance at design time.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Calculate the absolute path to the startup project's appsettings.json file.
            
            // --- ปรับปรุงการคำนวณ Base Path ให้มั่นคงขึ้น ---
            // 1. หาพาธของ Current Directory (น่าจะเป็น ProjectHub.Infrastructure/bin/Debug)
            var currentDirectory = Directory.GetCurrentDirectory();
            
            // 2. เลื่อนขึ้นไปยัง Solution Root โดยการวนลูปจนกว่าจะเจอ Solution (.sln) file
            // หรือเลื่อนขึ้น 2 ระดับ (จาก Infrastructure/bin/Debug ไปที่ Solution Root)
            
            // ใช้ Path.Combine เพื่อสร้างพาธที่คาดหวังไปยัง ProjectHub.API
            // เนื่องจากคุณรันคำสั่งจาก C:\Users\acre_tuajing\Desktop\khanun\ProjectHub (Solution Root)
            // เราสามารถใช้คำสั่งหา Root ได้ง่ายกว่า
            
            // วิธีที่ 1: ลองใช้ AppContext.BaseDirectory เพื่อหาสถานที่ของ DLL
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            // เนื่องจากเรารันจาก Solution Root และ ProjectHub.API อยู่ข้างๆ
            // เราสามารถสร้างพาธโดยอ้างอิงจาก Working Directory
            // (ซึ่งในกรณีที่คุณรันคำสั่งจาก Solution Root มันคือ 'C:\Users\acre_tuajing\Desktop\khanun\ProjectHub')
            var solutionRoot = currentDirectory; 
            
            // ตรวจสอบว่า .sln file อยู่ใน currentDirectory หรือไม่
            if (!File.Exists(Path.Combine(solutionRoot, "ProjectHub.sln")))
            {
                // ถ้าไม่เจอ .sln แสดงว่าเราอยู่ในโฟลเดอร์ Infrastructure หรือต่ำกว่า
                // เราต้องหา Solution Root โดยการเลื่อนขึ้นไปเรื่อยๆ
                var directory = new DirectoryInfo(currentDirectory);
                while (directory != null && !File.Exists(Path.Combine(directory.FullName, "ProjectHub.sln")))
                {
                    directory = directory.Parent;
                }
                solutionRoot = directory.FullName;
            }
            
            // 2. Set the Base Path to the ProjectHub.API folder
            var basePath = Path.Combine(solutionRoot, "ProjectHub.API");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .Build();

            // Retrieve the Connection String
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                // ถ้าหาไม่เจอ ให้แสดงพาธที่มันพยายามหาออกมา เพื่อช่วย Debug
                throw new InvalidOperationException($"DefaultConnection string not found in appsettings.json. Looked in: {Path.Combine(basePath, "appsettings.json")}");
            }

            // Create DbContextOptionsBuilder
            var builder = new DbContextOptionsBuilder<AppDbContext>();

            // Configure SQL Server
            builder.UseSqlServer(connectionString);

            // Return the DbContext instance to the EF Core tool
            return new AppDbContext(builder.Options);
        }
    }
}
