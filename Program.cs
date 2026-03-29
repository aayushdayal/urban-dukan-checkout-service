using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; // Optional, but sometimes needed for intellisense
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

// Logging included by default
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
    options.Ssl = cfg.UseSsl;
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

// AutoMapper is optional; small mapping done manually

var app = builder.Build();

// Apply DB migrations at startup (optional for production you'd handle migrations separately)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
