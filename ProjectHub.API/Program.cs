using System.Data;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using ProjectHub.API.Mapping;
using ProjectHub.Application.Features.Users.Login;
using ProjectHub.Application.Features.Users.Register;
using ProjectHub.Application.Interfaces;

using ProjectHub.Application.Mapping;
using ProjectHub.Application.Repositories;
using ProjectHub.Application.Services;
using ProjectHub.Infrastructure.Auth;
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Infrastructure.Repositories;
using System.Text.Json.Serialization;
using ProjectHub.Infrastructure;





var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("Microsoft", LogLevel.Information);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("ProjectHub", LogLevel.Debug);


builder.Services
  .AddControllers()
  .AddJsonOptions(o =>
  {
      o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
      o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  });


var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'PostgresConnection' not found in appsettings.json."
    );
}

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"---> Runtime ConnectionString Used: {connectionString}");
Console.ResetColor();


builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));


builder.Services.AddAutoMapper(
    cfg => { }, 
    typeof(ProjectProfile).Assembly, 
    typeof(ApiMappingProfile).Assembly, 
    typeof(ProjectHub.API.Mapping.ApiMappingProfile).Assembly,
    typeof(ProjectHub.Application.Mapping.ProjectProfile).Assembly
);


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
    cfg.Lifetime = ServiceLifetime.Scoped;
});


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>(); 
builder.Services.AddScoped<IColumnRepository, ColumnRepository>();
builder.Services.AddScoped<IRowRepository, RowRepository>();
builder.Services.AddScoped<IRelationshipRepository, RelationshipRepository>();
builder.Services.AddScoped<IFormulaTranslator, FormulaTranslator>(); 
builder.Services.AddScoped<IProjectSecurityService,
    ProjectHub.Infrastructure.Services.ProjectSecurityService>();
builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("PostgresConnection"))
);
builder.Services.AddScoped<IPasswordHasher, ProjectHub.Infrastructure.Services.PasswordHasher>();



builder.Services.AddControllers();


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


if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}


var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key not found in configuration (Jwt:Key)");
}

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
        };

        
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            { /* ... (Logging) ... */
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            { /* ... (Logging) ... */
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            { /* ... (Logging) ... */
                return Task.CompletedTask;
            },
        };
    });


builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();


builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


builder.Services.AddCors(opt =>
{
    opt.AddPolicy(
        "AllowLocal",
        p =>
            p.WithOrigins(
                    
                    "http://localhost:5254",
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "https://localhost:52212",
                    "http://localhost:52212",
                    "https://localhost:4200",
                    "http://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
app.UseCors("AllowLocal");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseAuthentication(); 
app.UseAuthorization(); 


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
