using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; // Optional, but sometimes needed for intellisense
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using StackExchange.Redis;
using urban_dukan_checkout_service.Clients;
using urban_dukan_checkout_service.Configurations;
using urban_dukan_checkout_service.Data;
using urban_dukan_checkout_service.Repositories;
using urban_dukan_checkout_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Clear default claim mapping so "sub" remains "sub"
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Configuration
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<ProductServiceSettings>(builder.Configuration.GetSection("ProductService"));

// JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Swagger - add JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

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
builder.Services.AddSingleton<ServiceBusPublisher>();

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
    // Allow non-HTTPS in Development for local testing (switch to true in production)
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment() ? true : false;
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
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

// AutoMapper is optional; small mapping done manually

var app = builder.Build();

// Apply DB migrations at startup (optional for production you'd handle migrations separately)
using (var scope = app.Services.CreateScope())
{
    var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
    Console.WriteLine($"Redis Connected: {redis.IsConnected}");
    //var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    //db.Database.Migrate();
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
