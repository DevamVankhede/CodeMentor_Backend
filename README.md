# CodeMentor AI - Backend API

.NET 8.0 Web API for CodeMentor AI platform.

## 🚀 Features

- **Authentication** - JWT-based auth with user management
- **AI Integration** - Code analysis and AI features
- **Roadmap System** - Learning roadmap management
- **Game System** - Coding challenges and games
- **Real-time Collaboration** - SignalR for live collaboration
- **Dashboard** - User statistics and progress tracking

## 🛠️ Tech Stack

- .NET 8.0
- Entity Framework Core
- SQLite (Development) / SQL Server (Production)
- SignalR
- JWT Authentication

## 📋 Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (for production) or SQLite (for development)

## 🏃 Quick Start

### 1. Clone and Navigate
\`\`\`bash
cd CodeMentorAI.API
\`\`\`

### 2. Restore Dependencies
\`\`\`bash
dotnet restore
\`\`\`

### 3. Update Database
\`\`\`bash
dotnet ef database update
\`\`\`

Or use the PowerShell script:
\`\`\`powershell
./update-database.ps1
\`\`\`

### 4. Run the API
\`\`\`bash
dotnet run
\`\`\`

The API will be available at:
- HTTPS: https://localhost:7000
- HTTP: http://localhost:5000

## 📁 Project Structure

\`\`\`
CodeMentorAI.API/
├── Controllers/          # API endpoints
│   ├── AuthController.cs
│   ├── AIController.cs
│   ├── RoadmapController.cs
│   ├── GameController.cs
│   └── ...
├── Models/              # Data models
│   ├── User.cs
│   ├── RoadmapModels.cs
│   └── GameModels.cs
├── DTOs/                # Data transfer objects
├── Services/            # Business logic
│   ├── AIService.cs
│   └── DataSeeder.cs
├── Data/                # Database context
│   └── AppDbContext.cs
├── Hubs/                # SignalR hubs
│   └── CollaborationHub.cs
├── Migrations/          # EF migrations
└── Program.cs           # Application entry point
\`\`\`

## 🔧 Configuration

### appsettings.json
\`\`\`json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=codementor_ai.db"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-characters",
    "Issuer": "CodeMentorAI",
    "Audience": "CodeMentorAI",
    "ExpirationInMinutes": 1440
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://your-frontend-url.com"
    ]
  }
}
\`\`\`

### Environment Variables (Production)
\`\`\`
ConnectionStrings__DefaultConnection=your_connection_string
JwtSettings__Secret=your_jwt_secret
JwtSettings__Issuer=https://your-api-url.com
ASPNETCORE_ENVIRONMENT=Production
\`\`\`

## 📡 API Endpoints

### Authentication
- \`POST /api/auth/signup\` - Register new user
- \`POST /api/auth/login\` - Login user
- \`GET /api/auth/me\` - Get current user
- \`PUT /api/auth/profile\` - Update profile

### AI Features
- \`POST /api/ai/analyze\` - Analyze code
- \`POST /api/ai/refactor\` - Refactor code
- \`POST /api/ai/explain\` - Explain code
- \`POST /api/ai/generate\` - Generate code

### Roadmaps
- \`GET /api/roadmap\` - Get all roadmaps
- \`GET /api/roadmap/{id}\` - Get roadmap by ID
- \`POST /api/roadmap\` - Create roadmap
- \`PUT /api/roadmap/{id}\` - Update roadmap
- \`DELETE /api/roadmap/{id}\` - Delete roadmap

### Games
- \`GET /api/game/challenges\` - Get challenges
- \`POST /api/game/submit\` - Submit solution
- \`GET /api/game/leaderboard\` - Get leaderboard

### Dashboard
- \`GET /api/dashboard/stats\` - Get user statistics
- \`GET /api/dashboard/activity\` - Get recent activity

## 🗄️ Database

### Development (SQLite)
The project uses SQLite by default for development. The database file is \`codementor_ai.db\`.

### Production (SQL Server)
Update the connection string in appsettings.json or environment variables:
\`\`\`
Server=your-server;Database=CodeMentorAI;User Id=your-user;Password=your-password;
\`\`\`

### Migrations
\`\`\`bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
\`\`\`

## 🚀 Deployment

### Azure App Service
\`\`\`bash
# Publish
dotnet publish -c Release -o ./publish

# Deploy using Azure CLI
az webapp deployment source config-zip \\
  --resource-group your-rg \\
  --name your-app-name \\
  --src ./publish.zip
\`\`\`

### Docker
\`\`\`dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CodeMentorAI.API.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeMentorAI.API.dll"]
\`\`\`

Build and run:
\`\`\`bash
docker build -t codementor-api .
docker run -p 5000:80 codementor-api
\`\`\`

## 🔐 Security

- JWT authentication required for protected endpoints
- CORS configured for allowed origins
- Input validation on all endpoints
- SQL injection protection via EF Core
- Password hashing with BCrypt

## 🧪 Testing

\`\`\`bash
# Run tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
\`\`\`

## 📊 Monitoring

### Health Check
\`\`\`bash
curl https://localhost:7000/health
\`\`\`

### Logging
Logs are written to:
- Console (Development)
- Application Insights (Production)
- File system (configurable)

## 🆘 Troubleshooting

### Database Connection Issues
- Check connection string in appsettings.json
- Ensure database server is running
- Verify firewall rules

### CORS Errors
- Add frontend URL to allowed origins in appsettings.json
- Restart the API after configuration changes

### Migration Issues
\`\`\`bash
# Reset database
dotnet ef database drop
dotnet ef database update
\`\`\`

## 📝 License

MIT License

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## 📞 Support

For issues and questions:
- Open an issue on GitHub
- Check documentation
- Contact: support@codementor-ai.com
