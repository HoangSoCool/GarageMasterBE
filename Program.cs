using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────────────────────────
// 1. ĐỌC CẤU HÌNH & BIẾN MÔI TRƯỜNG
// ────────────────────────────────────────────────────────────────
var configuration = builder.Configuration;

string mongoUri =
    Environment.GetEnvironmentVariable("MONGODB_URI")       // Render / Docker
    ?? configuration["MongoDB:ConnectionString"];           // appsettings.json

if (string.IsNullOrWhiteSpace(mongoUri))
    throw new InvalidOperationException("❌ Thiếu MONGODB_URI!");

string jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? configuration["JwtSettings:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("❌ Thiếu JWT_SECRET!");

// ────────────────────────────────────────────────────────────────
// 2. ĐĂNG KÝ OPTIONS
// ────────────────────────────────────────────────────────────────
builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

builder.Services.Configure<JwtSettings>(opts =>
{
    configuration.GetSection("JwtSettings").Bind(opts);
    opts.SecretKey = jwtSecret;                             // ghi đè = env
});

builder.Services.Configure<MongoDBSettings>(opts =>
{
    configuration.GetSection("MongoDB").Bind(opts);
    opts.ConnectionString = mongoUri;                       // ghi đè = env
});

// ────────────────────────────────────────────────────────────────
// 3. MONGODB – IMongoClient + DbContext
// ────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUri));

builder.Services.AddSingleton(sp =>
{
    var mongoCfg = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client   = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoCfg.DatabaseName);
});

builder.Services.AddSingleton<MongoDbContext>();

// ────────────────────────────────────────────────────────────────
// 4. CORS – cho FE localhost & FE trên Render
// ────────────────────────────────────────────────────────────────
var allowedOrigins = new[]
{
    "http://localhost:5173",              // FE dev (Vite)
    "https://garagemasterfe.onrender.com" // FE prod (sửa đúng domain FE của bạn)
};

builder.Services.AddCors(o =>
    o.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

// ────────────────────────────────────────────────────────────────
// 5. AUTHENTICATION – JWT Bearer
// ────────────────────────────────────────────────────────────────
var jwtOptions = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = jwtOptions.Issuer,

        ValidateAudience         = true,
        ValidAudience            = jwtOptions.Audience,

        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// ────────────────────────────────────────────────────────────────
// 6. DI – ĐĂNG KÝ SERVICE
// ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<PartsService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<RepairOrderService>();
builder.Services.AddScoped<RepairDetailService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddSingleton<JwtService>();

// ────────────────────────────────────────────────────────────────
// 7. MVC + SWAGGER (luôn bật ở mọi môi trường)
// ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "GarageMaster API",
        Version     = "v1",
        Description = "API quản lý tiệm sửa xe GarageMaster"
    });

    // Thêm nút nhập JWT vào Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token ở format: Bearer {token}",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ────────────────────────────────────────────────────────────────
// 8. LOGGING – console (Render đủ dùng)
// ────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ────────────────────────────────────────────────────────────────
// 9. BUILD PIPELINE
// ────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors("AllowFrontend");

// 👉 Luôn bật Swagger (UI tại /docs)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarageMaster API v1");
    c.RoutePrefix = "docs";                  // => https://.../docs
});

app.MapGet("/", () =>
    Results.Text("🚗 GarageMaster API is running!", "text/plain"));  // 👈 tránh 404

app.MapGet("/healthz", () => Results.Ok("Healthy ✅"));              // health check

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
