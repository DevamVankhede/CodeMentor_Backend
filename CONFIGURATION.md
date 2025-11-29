# Backend Configuration Guide

## 📋 Configuration Files

Your .NET backend uses these configuration files:

1. **appsettings.json** - Base configuration (committed to Git)
2. **appsettings.Development.json** - Development overrides (committed to Git)
3. **appsettings.Production.json** - Production template (committed to Git)
4. **Environment Variables** - Production secrets (NOT in Git)

## 🔐 Current Configuration

### appsettings.json
Contains your base configuration with default values.

**⚠️ IMPORTANT**: The Google AI API key in this file should be removed before deploying!

### What's in Your Config:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=codementor_ai.db"
  },
  "Jwt": {
    "Secret": "your-super-secret-key...",
    "Issuer": "CodeMentorAI",
    "Audience": "CodeMentorAI"
  },
  "Cors": {
    "AllowedOrigin": "http://localhost:3000"
  },
  "GoogleAI": {
    "ApiKey": "AIzaSy..."
  }
}
```

## 🚀 For Render Deployment

### Environment Variables to Set:

When deploying to Render, add these environment variables:

```bash
# Database (if using PostgreSQL)
ConnectionStrings__DefaultConnection=postgresql://user:password@host/database

# JWT Settings
Jwt__Secret=your-super-secret-jwt-key-min-32-characters-long
Jwt__Issuer=https://codementor-api.onrender.com
Jwt__Audience=CodeMentorAI

# CORS (your frontend URL)
Cors__AllowedOrigin=https://your-frontend.vercel.app

# Google AI API Key
GoogleAI__ApiKey=AIzaSyDrTOl4IrjwQ-DIgIPlLT5o7XoL-I4RcKo

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
```

### How to Add in Render:

1. Go to your service in Render dashboard
2. Click "Environment" tab
3. Add each variable:
   - Key: `ConnectionStrings__DefaultConnection`
   - Value: Your database connection string
4. Click "Save Changes"

**Note**: Use double underscores `__` to represent nested JSON properties.

## 🔧 Configuration Priority

.NET reads configuration in this order (later overrides earlier):

1. appsettings.json
2. appsettings.{Environment}.json
3. Environment Variables
4. Command-line arguments

So environment variables will override appsettings.json values.

## 📝 Example: Setting Up on Render

### Step 1: Create render.yaml

In your **project root**, create `render.yaml`:

```yaml
services:
  - type: web
    name: codementor-api
    runtime: docker
    dockerfilePath: ./CodeMentorAI.API/Dockerfile
    dockerContext: ./CodeMentorAI.API
    plan: free
    branch: main
    healthCheckPath: /health
    envVars:
      # ASP.NET Core
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ASPNETCORE_URLS
        value: http://+:80
      
      # JWT Settings
      - key: Jwt__Secret
        generateValue: true  # Render will generate a secure random value
      - key: Jwt__Issuer
        value: https://codementor-api.onrender.com
      - key: Jwt__Audience
        value: CodeMentorAI
      
      # CORS
      - key: Cors__AllowedOrigin
        value: https://your-frontend.vercel.app
      
      # Google AI
      - key: GoogleAI__ApiKey
        value: AIzaSyDrTOl4IrjwQ-DIgIPlLT5o7XoL-I4RcKo
      
      # Database (if using PostgreSQL)
      - key: ConnectionStrings__DefaultConnection
        fromDatabase:
          name: codementor-db
          property: connectionString

databases:
  - name: codementor-db
    databaseName: codementor
    user: codementor
    plan: free
```

### Step 2: Update Program.cs (if needed)

Make sure your Program.cs reads from configuration:

```csharp
// JWT Secret
var jwtSecret = builder.Configuration["Jwt:Secret"];

// CORS Origin
var corsOrigin = builder.Configuration["Cors:AllowedOrigin"];

// Google AI Key
var googleAiKey = builder.Configuration["GoogleAI:ApiKey"];
```

## 🔒 Security Best Practices

### ✅ DO:
- Use environment variables for secrets in production
- Use strong, random JWT secrets (32+ characters)
- Keep appsettings.json with placeholder values
- Use different secrets for dev and production

### ❌ DON'T:
- Commit real API keys to Git
- Use the same JWT secret in dev and production
- Hardcode secrets in code
- Share production secrets

## 🧪 Testing Configuration

### Local Development:
```bash
cd CodeMentorAI.API
dotnet run
```

Uses: `appsettings.Development.json`

### Production (Render):
Uses: Environment variables you set in Render dashboard

### Verify Configuration:
Add this to your Program.cs to log configuration (remove in production):

```csharp
// Log configuration (for debugging only)
var jwtSecret = builder.Configuration["Jwt:Secret"];
Console.WriteLine($"JWT Secret configured: {!string.IsNullOrEmpty(jwtSecret)}");
Console.WriteLine($"JWT Secret length: {jwtSecret?.Length ?? 0}");

var googleKey = builder.Configuration["GoogleAI:ApiKey"];
Console.WriteLine($"Google AI Key configured: {!string.IsNullOrEmpty(googleKey)}");
```

## 📊 Configuration Checklist

Before deploying:

- [ ] Remove or replace real API keys in appsettings.json
- [ ] Create appsettings.Production.json with empty values
- [ ] Create render.yaml with environment variables
- [ ] Set all required environment variables in Render
- [ ] Test locally with Development settings
- [ ] Verify CORS origin matches your frontend URL
- [ ] Use strong JWT secret (32+ characters)

## 🔄 Updating Configuration

### To update a setting:

1. **In Render Dashboard:**
   - Go to Environment tab
   - Update the variable
   - Click "Save Changes"
   - Service will automatically redeploy

2. **In render.yaml:**
   - Update the value
   - Commit and push to GitHub
   - Render will redeploy automatically

## 🆘 Troubleshooting

### Issue: "Configuration value not found"

**Check:**
- Environment variable name uses `__` (double underscore)
- Variable is set in Render dashboard
- Service was restarted after adding variable

### Issue: "Invalid JWT secret"

**Solution:**
- Ensure JWT secret is at least 32 characters
- Check for typos in environment variable name
- Verify secret is set in Render

### Issue: CORS errors

**Solution:**
- Update `Cors__AllowedOrigin` to match your frontend URL
- Include protocol (https://)
- No trailing slash

## 📞 Need Help?

Check these files:
- `DEPLOY_BACKEND_RENDER.md` - Complete deployment guide
- `README.md` - Backend overview
- `.env.example` - Example environment variables

---

**Your configuration is ready for deployment!** 🚀
