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
// 1. ĐỌC CẤU HÌNH & KIỂM TRA THIẾU
// ────────────────────────────────────────────────────────────────
var configuration = builder.Configuration;

// MongoDB URI: ưu tiên biến môi trường, fallback sang appsettings
var mongoUri = Environment.GetEnvironmentVariable("MONGODB_URI")
            ?? configuration["MongoDB:ConnectionString"];

if (string.IsNullOrWhiteSpace(mongoUri))
    throw new InvalidOperationException(
        "❌ MongoDB connection string chưa được cấu hình!");

// JWT secret – đọc env trước
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
              ?? configuration["JwtSettings:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException(
        "❌ JWT secret key chưa được cấu hình!");

// ────────────────────────────────────────────────────────────────
// 2. ĐĂNG KÝ CÁC OPTIONS
// ────────────────────────────────────────────────────────────────
builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
builder.Services.Configure<JwtSettings>(opts =>
{
    configuration.GetSection("JwtSettings").Bind(opts);
    opts.SecretKey = jwtSecret;           // ghi đè secret từ env
});
builder.Services.Configure<MongoDBSettings>(opts =>
{
    configuration.GetSection("MongoDB").Bind(opts);
    opts.ConnectionString = mongoUri;     // ghi đè URI từ env
});

// ────────────────────────────────────────────────────────────────
// 3. MONGODB (IMongoDatabase + DbContext)
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
// 4. CORS – cho FE localhost & Render
// ────────────────────────────────────────────────────────────────
var allowedOrigins = new[]
{
    "http://localhost:5173",          // dev
    "https://my-fe.onrender.com"      // prod
};
builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowFrontend", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

// ────────────────────────────────────────────────────────────────
// 5. AUTHENTICATION – JWT BEARER (.NET 8)
// ────────────────────────────────────────────────────────────────
var jwtOpts = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = jwtOpts.Issuer,

        ValidateAudience         = true,
        ValidAudience            = jwtOpts.Audience,

        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// ────────────────────────────────────────────────────────────────
// 6. ĐĂNG KÝ SERVICE DI 
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
// 7. MVC + SWAGGER
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

    // JWT trong Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT theo dạng: Bearer {token}",
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
// 8. LOGGING (console đủ cho Render)
// ────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ────────────────────────────────────────────────────────────────
// 9. PIPELINE
// ────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarageMaster API v1");
        c.RoutePrefix = string.Empty;               // Swagger ở /
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint health check đơn giản (Render health check)
app.MapGet("/healthz", () => Results.Ok("Healthy ✅"));

app.Run();
