using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Fido2NetLib;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.Repositories;
using backend.Services;
using PayOS;
using Hangfire;
using Hangfire.MySql;
using MySqlConnector;
using Backend.Services;


var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];

Console.WriteLine("HAS_DB_CONNECTION: " + !string.IsNullOrWhiteSpace(connectionString));

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Missing ConnectionStrings__DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Missing MYSQL_DATABASE");
}

var cs = connectionString;
Console.WriteLine("DB_HOST_TEST: " + cs);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<GmailOptions>(
    builder.Configuration.GetSection(GmailOptions.GmailOptionKey)
);
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(
    connectionString,
    ServerVersion.AutoDetect(connectionString)
));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
        ),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddHangfire(config =>
{
    config.UseStorage(new MySqlStorage(
    connectionString,
    new MySqlStorageOptions()
));
});

builder.Services.AddHangfireServer();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
        "http://localhost:3000",
        "https://intern-easy-salon-authen-system.vercel.app"
      )
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
    });
});



builder.Services.AddSingleton<Fido2>(sp =>
{
    return new Fido2(new Fido2Configuration
    {
        ServerDomain = "intern-easy-salon-authen-system.vercel.app",
        ServerName = "TechAI",
        Origins = new HashSet<string>
        {
            "https://intern-easy-salon-authen-system.vercel.app"
        }
    });
});


builder.Services.AddSingleton<PayOSClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    return new PayOSClient(
        config["PayOS:ClientId"],
        config["PayOS:ApiKey"],
        config["PayOS:ChecksumKey"]
    );
});

builder.Services.AddControllers();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPasskeyService, PasskeyService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IInventoryDocumentService, InventoryDocumentService>();
builder.Services.AddScoped<IAiChatRepository, AiChatRepository>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<IProductRetrievalService, ProductRetrievalService>();
builder.Services.AddHttpClient<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IPromptBuilderService, PromptBuilderService>();
builder.Services.AddScoped<IIntentDetectionService, IntentDetectionService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<ISemanticSearchService, SemanticSearchService>();
builder.Services.AddScoped<IHybridSearchService, HybridSearchService>();
builder.Services.AddScoped<IConversationMemoryService, ConversationMemoryService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddHostedService<ExpiredPaymentBackgroundService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<InvoiceEmailService>();
builder.Services.AddScoped<ITaxService, TaxService>();


builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtOptions"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard();

app.MapHub<NotificationHub>("/notificationHub");


// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
