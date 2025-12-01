using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.Services;
using CodeMentorAI.API.Hubs;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure port binding early - Render requires this
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    // Render/Cloud: Use PORT from environment
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Console.WriteLine($"🔧 Configuring port binding: http://0.0.0.0:{port}");
}
else if (!builder.Environment.IsDevelopment())
{
    // Production without PORT: Use default
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
    Console.WriteLine("🔧 Configuring port binding: http://0.0.0.0:8080");
}

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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Data Source=codementor_ai.db";
}

var configuredProvider = builder.Configuration["Database:Provider"];
var databaseProvider = DetectDatabaseProvider(configuredProvider, connectionString);

// Ensure SQLite paths are always absolute so data stays on disk across restarts
if (databaseProvider == "sqlite")
{
    connectionString = NormalizeSqliteConnectionString(connectionString, builder.Environment.ContentRootPath);
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (databaseProvider == "postgres")
    {
        options.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });
    }
    else
    {
        options.UseSqlite(connectionString);
    }

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

// Ensure database is created/migrated and seeded (non-blocking for startup)
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        await context.Database.MigrateAsync();
        logger.LogInformation("✅ Database migrations applied successfully");
        
        await CodeMentorAI.API.Services.DataSeeder.SeedAsync(context, logger);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error occurred while creating/seeding the database");
    }
});

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

// Log startup info
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("🚀 CodeMentor AI Backend Starting (Development)...");
    Console.WriteLine($"🗄️  Database provider: {databaseProvider}");
    Console.WriteLine($"📁 Connection string: {connectionString}");
}
else
{
    var configuredPort = port ?? "8080";
    Console.WriteLine($"🚀 CodeMentor AI Backend Starting (Production)...");
    Console.WriteLine($"🌐 Listening on: http://0.0.0.0:{configuredPort}");
}

Console.WriteLine("✨ Ready for connections!");

app.Run();

static string DetectDatabaseProvider(string? configuredProvider, string connectionString)
{
    if (!string.IsNullOrWhiteSpace(configuredProvider))
    {
        return configuredProvider.Trim().ToLowerInvariant();
    }

    if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("postgres", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase))
    {
        return "postgres";
    }

    return "sqlite";
}

static string NormalizeSqliteConnectionString(string connectionString, string contentRoot)
{
    const string dataSourcePrefix = "Data Source=";
    if (!connectionString.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase))
    {
        return connectionString;
    }

    var dataSource = connectionString.Substring(dataSourcePrefix.Length).Trim().Trim('"');
    if (Path.IsPathRooted(dataSource))
    {
        return connectionString;
    }

    var absolutePath = Path.Combine(contentRoot, dataSource);
    var directory = Path.GetDirectoryName(absolutePath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    return $"{dataSourcePrefix}{absolutePath}";
}