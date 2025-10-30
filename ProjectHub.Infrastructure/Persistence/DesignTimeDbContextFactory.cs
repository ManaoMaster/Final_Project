// ใน ProjectHub.Infrastructure/Persistence/DesignTimeDbContextFactory.cs
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProjectHub.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Logic การหา Solution Root และ API Path
            string basePath = Directory.GetCurrentDirectory(); // "...\ProjectHub.Infrastructure"
            string solutionRoot = Directory.GetParent(basePath)!.FullName; // "...\Final_Project"
            string apiProjectPath = Path.Combine(solutionRoot, "ProjectHub.API");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath) // อ่านจาก API Project
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // *** 1. อ่าน Connection String ใหม่ (ของ PostgreSQL) ***
            // (เราจะใช้ชื่อใหม่ "PostgresConnection" เพื่อความชัดเจน)
            var connectionString = configuration.GetConnectionString("PostgresConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'PostgresConnection' not found in appsettings.json of API project."
                );
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // *** 2. เปลี่ยนจาก UseSqlite เป็น UseNpgsql ***
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
