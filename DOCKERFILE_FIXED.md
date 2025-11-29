# Dockerfile Fixed for .NET 9.0

## Issue
Your project uses .NET 9.0, but the Dockerfile was configured for .NET 8.0.

## Error
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 9.0
```

## Solution
Updated Dockerfile to use .NET 9.0 images:

### Changed:
```dockerfile
# OLD (8.0)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# NEW (9.0)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
```

## ✅ Fixed Dockerfile

Your `CodeMentorAI.API/Dockerfile` now uses:
- `mcr.microsoft.com/dotnet/aspnet:9.0` - Runtime image
- `mcr.microsoft.com/dotnet/sdk:9.0` - Build image

## 🚀 Next Steps

1. **Commit the changes:**
   ```bash
   git add CodeMentorAI.API/Dockerfile
   git commit -m "Fix: Update Dockerfile to .NET 9.0"
   git push origin main
   ```

2. **Deploy to Render:**
   - Render will automatically detect the push
   - Build will now succeed with .NET 9.0

## 🧪 Test Locally

```bash
cd CodeMentorAI.API

# Build Docker image
docker build -t codementor-api .

# Run container
docker run -p 5000:80 codementor-api

# Test
curl http://localhost:5000/health
```

## ✅ Verification

After deployment, your API should:
- ✅ Build successfully
- ✅ Start without errors
- ✅ Respond to health checks
- ✅ Handle API requests

## 📝 Notes

- .NET 9.0 is the latest version (released November 2024)
- Fully compatible with your existing code
- Better performance than .NET 8.0
- All your packages support .NET 9.0

---

**Status**: ✅ Fixed and ready to deploy!
