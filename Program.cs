using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//Deploy
var connectionString = builder.Configuration["MongoDB:ConnectionString"];
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("MongoDB connection string is not configured. Please check your appsettings.json file.");
}
var client = new MongoClient(connectionString);
var database = client.GetDatabase(builder.Configuration["MongoDB:DatabaseName"] ?? "GarageMasterDB");

builder.Services.AddSingleton<IMongoClient>(client);

builder.Services.AddCors(p => p.AddPolicy("AllowFE",
    b => b.WithOrigins("https://my-fe.onrender.com")
          .AllowAnyHeader()
          .AllowAnyMethod()));
// Đọc config JwtSettings từ appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// Đăng ký JwtSettings để inject IOptions<JwtSettings>
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Đăng ký cấu hình MongoDBSettings từ appsettings.json
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Đăng ký IMongoClient singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Đăng ký MongoDbContext singleton (nếu bạn có class này dùng riêng)
builder.Services.AddSingleton<MongoDbContext>();

// Đăng ký IMongoDatabase để inject vào service khác
builder.Services.AddScoped(sp => sp.GetRequiredService<MongoDbContext>().Database);

// Đăng ký UserService (scoped vì có async và tương tác DB)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();

// Đăng ký BrandService và PartsService
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

// Đăng ký JwtService dùng để tạo token (singleton hoặc scoped cũng được)
builder.Services.AddSingleton<JwtService>();

// Cấu hình CORS để cho phép frontend truy cập API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Địa chỉ frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // Nếu frontend gửi cookie/token với credential
    });
});

// Đăng ký Authentication với JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

        ClockSkew = TimeSpan.Zero
    };
});

// Đăng ký controller
builder.Services.AddControllers();

// Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GarageMaster API",
        Version = "v1",
        Description = "API quản lý tiệm sửa xe GarageMaster"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Cấu hình logging (mặc định console)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
app.UseCors("AllowFE");

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarageMaster API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Kích hoạt CORS trước Authentication & Authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
