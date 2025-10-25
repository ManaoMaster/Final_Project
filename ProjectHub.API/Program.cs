using MediatR; // สำหรับ AddMediatR
using Microsoft.EntityFrameworkCore;
// --- เพิ่ม Using Statements ที่จำเป็น ---
using ProjectHub.Application.Features.Users.Register; // สำหรับ MediatR Assembly Scan
using ProjectHub.Application.Repositories; // สำหรับ IUserRepository
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Infrastructure.Repositories; // สำหรับ UserRepository

// (ลบ using MediatR; ที่ซ้ำซ้อนออก 1 บรรทัด เพื่อแก้ Warning CS0105)

var builder = WebApplication.CreateBuilder(args);

// --- 1. ตั้งค่า SQLite Database (แก้ไข Path) ---
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") 
    // *** แก้ไข: เพิ่ม ../ เพื่อชี้ไปยัง Root Folder ***
    ?? "Data Source=../ProjectHub.db"; 

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// --- 2. ลงทะเบียน MediatR (แก้ไข синтаксис v12) ---
builder.Services.AddMediatR(
    cfg =>
    {
        // บอกให้ MediatR ค้นหา Handlers ใน Assembly ของ RegisterUserCommand (Application Layer)
        cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
        
        // *** แก้ไข CS1501: ย้าย Lifetime เข้ามาข้างใน cfg ***
        cfg.Lifetime = ServiceLifetime.Scoped; 
    }
); // <-- ไม่มี Argument ที่ 2 ที่อยู่นอกวงเล็บนี้

// --- 3. ลงทะเบียน Repositories (ที่ขาดหายไป) ---
// เชื่อม Application Interface (IUserRepository) เข้ากับ Infrastructure Implementation (UserRepository)
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>(); // (Comment ไว้ก่อน ถ้ายังไม่สร้าง)

// --- 4. ลงทะเบียน Controllers (ที่คุณมีอยู่แล้ว) ---
builder.Services.AddControllers();

// --- 5. เพิ่ม Swagger (แนะนำสำหรับ API) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 6. ตั้งค่า Pipeline (เพิ่ม Swagger UI) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // (แนะนำให้มี)
app.UseAuthorization(); // (แนะนำให้มี)

app.MapControllers();

app.Run();
