using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; // Optional, but sometimes needed for intellisense
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using urban_dukan_checkout_service.Clients;
using urban_dukan_checkout_service.Configurations;
using urban_dukan_checkout_service.Data;
using urban_dukan_checkout_service.Repositories;
using urban_dukan_checkout_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<ProductServiceSettings>(builder.Configuration.GetSection("ProductService"));

// JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext - Orders only
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

// Redis ConnectionMultiplexer singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var cfg = builder.Configuration.GetSection("Redis").Get<RedisSettings>()
              ?? throw new InvalidOperationException("Redis configuration missing");
    var options = ConfigurationOptions.Parse(cfg.Configuration);

    options.AbortOnConnectFail = false;
    options.ConnectRetry = 3;
    options.ConnectTimeout = 5000;
    options.SyncTimeout = 5000;
    return ConnectionMultiplexer.Connect(options);
});

// Repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ProductService typed HttpClient
builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IConfiguration>().GetSection("ProductService").Get<ProductServiceSettings>();
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// JWT authentication
// Add a "Jwt" section to appsettings with Key, Issuer and Audience (Key required for symmetric signing).
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSection.GetValue<string>("Secret");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");

if (string.IsNullOrEmpty(jwtKey))
{
    // Fail fast in development so misconfiguration is obvious.
    throw new InvalidOperationException("JWT configuration missing. Please set Jwt:Key in configuration.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
        ValidIssuer = jwtIssuer,
        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(60)
    };
});

// AutoMapper is optional; small mapping done manually

var app = builder.Build();

// Apply DB migrations at startup (optional for production you'd handle migrations separately)
using (var scope = app.Services.CreateScope())
{
    var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
    Console.WriteLine($"Redis Connected: {redis.IsConnected}");
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

// Configure middleware

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Authentication must be before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
