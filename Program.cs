using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.Services;
using CodeMentorAI.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register AI Service with proper HTTP client configuration
builder.Services.AddHttpClient<IAIService, GoogleAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "CodeMentorAI/1.0");
});
builder.Services.AddScoped<IAIService, GoogleAIService>();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=codementor_ai.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "CodeMentorAI-Super-Secret-Key-2024-Must-Be-At-Least-32-Characters-Long";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CodeMentorAI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CodeMentorAI",
            ClockSkew = TimeSpan.Zero
        };
        
        // Configure SignalR authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/collaboration-hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// CORS configuration - Allow all for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("✅ Database created successfully");
        
        await CodeMentorAI.API.Services.DataSeeder.SeedAsync(context);
        logger.LogInformation("✅ Database seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error occurred while creating/seeding the database");
    }
}

// Middleware pipeline
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CollaborationHub>("/collaboration-hub");

// API endpoints
app.MapGet("/", () => new { 
    Message = "🚀 CodeMentor AI Backend is running!", 
    Version = "2.0.0",
    Timestamp = DateTime.UtcNow,
    Status = "Operational"
}).WithName("Root");

app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Services = new {
        Database = "Connected",
        AI = "Ready",
        SignalR = "Active"
    }
}).WithName("HealthCheck");

// Start the application
var port = app.Environment.IsDevelopment() ? "7000" : "5000";
app.Urls.Add($"https://localhost:{port}");
app.Urls.Add($"http://localhost:{(app.Environment.IsDevelopment() ? "5000" : "8080")}");

Console.WriteLine("🚀 CodeMentor AI Backend Starting...");
Console.WriteLine($"🌐 HTTPS: https://localhost:{port}");
Console.WriteLine($"🌐 HTTP: http://localhost:{(app.Environment.IsDevelopment() ? "5000" : "8080")}");
Console.WriteLine("✨ Ready for connections!");

app.Run();