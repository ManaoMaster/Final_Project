using System.Reflection;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MediatR; // สำหรับ AddMediatR
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
// LOGGING: ADDED
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectHub.API.Mapping; // ApiMappingProfile
using ProjectHub.Application.Features.Users.Login;
// --- เพิ่ม Using Statements ที่จำเป็น ---
using ProjectHub.Application.Features.Users.Register; // สำหรับ MediatR Assembly Scan
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Mapping; // ProjectProfile, UserProfile
using ProjectHub.Application.Repositories; // เปิดรายละเอียด error ของ JWT ตอน DEV
using ProjectHub.Infrastructure.Auth;
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Infrastructure.Repositories; // สำหรับ UserRepository

// (ลบ using MediatR; ที่ซ้ำซ้อนออก 1 บรรทัด เพื่อแก้ Warning CS0105)

var builder = WebApplication.CreateBuilder(args);

// --- 1. ตั้งค่า SQLite Database (แก้ไข Path) ---
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    // *** แก้ไข: เพิ่ม ../ เพื่อชี้ไปยัง Root Folder ***
    ?? "Data Source=../ProjectHub.db";

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"---> Runtime ConnectionString Used: {connectionString}");
Console.ResetColor();

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
builder.Services.AddScoped<IRowRepository, RowRepository>();

// --- 4. ลงทะเบียน Controllers (ที่คุณมีอยู่แล้ว) ---
builder.Services.AddControllers();

// --- 5. เพิ่ม Swagger (แนะนำสำหรับ API) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var jwt = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Put **ONLY** your JWT here",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    c.AddSecurityDefinition("Bearer", jwt);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwt, Array.Empty<string>() } });
});

// LOGGING: ADDED (เปิดข้อความ error รายละเอียดของ JWT—ใช้เฉพาะตอน DEV)
IdentityModelEventSource.ShowPII = true;

//Jwt
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            // ช่วยให้ validate หมดอายุตรงเป๊ะ (ไม่มี +5 นาที)
            ClockSkew = TimeSpan.Zero,

            // บอกระบบว่า claim ไหนคือ Name/Role (ให้ User.Identity.Name ใช้งานได้)
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
        };

        // LOGGING: ADDED — บันทึกสาเหตุ 401/Challenge/ไม่มี Header
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var log = ctx
                    .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JWT");

                // 1) มี Authorization header ไหม
                var hasAuth = ctx.Request.Headers.ContainsKey("Authorization");
                var authRaw = hasAuth ? ctx.Request.Headers["Authorization"].ToString() : "(none)";
                log.LogDebug("Authorization present: {Has} | Raw='{Auth}'", hasAuth, authRaw);

                // 2) header ขึ้นต้นด้วย Bearer ไหม และความยาว token เท่าไหร่
                if (hasAuth && authRaw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authRaw.Substring("Bearer ".Length).Trim();
                    log.LogDebug(
                        "Bearer token length: {Len} | head='{Head}'",
                        token.Length,
                        token.Length >= 12 ? token.Substring(0, 12) : token
                    );

                    // 3) ลอง decode แบบไม่ validate เพื่อดู iss/aud/exp (ช่วยจับ paste ผิด env)
                    try
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(token);
                        var iss = jwt.Issuer;
                        var aud = string.Join(",", jwt.Audiences ?? Array.Empty<string>());
                        var exp = jwt.ValidTo; // UTC
                        log.LogInformation(
                            "JWT parsed: iss='{Iss}' aud='{Aud}' exp(UTC)={Exp:O}",
                            iss,
                            aud,
                            exp
                        );
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning(ex, "Cannot parse JWT from header.");
                    }
                }
                else
                {
                    log.LogWarning("Authorization header missing or not 'Bearer <token>' format.");
                }
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = ctx =>
            {
                var log = ctx
                    .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JWT");
                log.LogError(
                    ctx.Exception,
                    "JWT auth failed (signature/lifetime/issuer/audience?)"
                );
                return Task.CompletedTask;
            },

            OnChallenge = ctx =>
            {
                var log = ctx
                    .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JWT");
                log.LogWarning("JWT challenge: {Error} - {Desc}", ctx.Error, ctx.ErrorDescription);
                return Task.CompletedTask;
            },
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
                    "https://localhost:52212",
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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// LOGGING: ADDED — log ค่าคอนฟิกที่โหลดมา เพื่อเช็คว่าอ่านชุดไหนอยู่ (ไม่โชว์ key จริง)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CFG");
    var issuer = app.Configuration["Jwt:Issuer"];
    var audience = app.Configuration["Jwt:Audience"];
    var key = app.Configuration["Jwt:Key"] ?? string.Empty;
    logger.LogInformation(
        "JWT cfg -> iss={Issuer}, aud={Audience}, keyLen={Len}",
        issuer,
        audience,
        key.Length
    );
}

app.MapControllers();

app.Run();
