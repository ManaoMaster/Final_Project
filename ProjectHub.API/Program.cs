using AutoMapper;
using MediatR; // สำหรับ AddMediatR
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProjectHub.API.Mapping; // ApiMappingProfile
// --- เพิ่ม Using Statements ที่จำเป็น ---
using ProjectHub.Application.Features.Users.Register; // สำหรับ MediatR Assembly Scan
using ProjectHub.Application.Mapping; // ProjectProfile, UserProfile
using ProjectHub.Application.Repositories; // สำหรับ IUserRepository
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Infrastructure.Repositories; // สำหรับ UserRepository
using System.Reflection;
using System.Text;
using ProjectHub.Application.Features.Users.Login;
using ProjectHub.Infrastructure.Auth;


// (ลบ using MediatR; ที่ซ้ำซ้อนออก 1 บรรทัด เพื่อแก้ Warning CS0105)

var builder = WebApplication.CreateBuilder(args);

// --- 1. ตั้งค่า SQLite Database (แก้ไข Path) ---
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    // *** แก้ไข: เพิ่ม ../ เพื่อชี้ไปยัง Root Folder ***
    ?? "Data Source=../ProjectHub.db";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));



// ลงทะเบียน AutoMapper โดยชี้ไปยัง assembly ของ Application
builder.Services.AddAutoMapper(
    cfg => { }, // บังคับให้เลือก overload ที่ถูก
    typeof(ProjectProfile).Assembly,
    typeof(ApiMappingProfile).Assembly
);



// --- 2. ลงทะเบียน MediatR (แก้ไข синтаксис v12) ---
builder.Services.AddMediatR(cfg =>
{
    // บอกให้ MediatR ค้นหา Handlers ใน Assembly ของ RegisterUserCommand (Application Layer)
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);

    // *** แก้ไข CS1501: ย้าย Lifetime เข้ามาข้างใน cfg ***
    cfg.Lifetime = ServiceLifetime.Scoped;
}); // <-- ไม่มี Argument ที่ 2 ที่อยู่นอกวงเล็บนี้



// --- 3. ลงทะเบียน Repositories (ที่ขาดหายไป) ---
// เชื่อม Application Interface (IUserRepository) เข้ากับ Infrastructure Implementation (UserRepository)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>(); // (Comment ไว้ก่อน ถ้ายังไม่สร้าง)
builder.Services.AddScoped<IColumnRepository, ColumnRepository>();


// --- 4. ลงทะเบียน Controllers (ที่คุณมีอยู่แล้ว) ---
builder.Services.AddControllers();

// --- 5. เพิ่ม Swagger (แนะนำสำหรับ API) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Jwt
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// Token service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(
        "AllowLocal",
        p =>
            p.WithOrigins(
                    "https://localhost:7069", // ถ้าเรียกจาก Swagger ของ API เอง ไม่ต้องใส่
                    "http://localhost:5254", // พอร์ต http ของ API (ถ้าใช้)
                    "http://localhost:5173", // << ถ้ามี Frontend Vite/React แยกพอร์ต ให้ใส่ที่นี่
                    "http://localhost:3000", // << ตัวอย่างเพิ่ม origin อื่น
                    "http://localhost:52212"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
    // .AllowCredentials() // ถ้ามี cookie/auth
    );
});

var app = builder.Build();

// --- 6. ตั้งค่า Pipeline (เพิ่ม Swagger UI) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowLocal");
app.UseHttpsRedirection(); 
app.UseAuthorization(); 
app.UseAuthentication();

app.MapControllers();

app.Run();
