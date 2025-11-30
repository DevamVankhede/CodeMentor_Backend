# Multi-stage build for .NET 8.0 API - Optimized for Render
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CodeMentorAI.API.csproj", "./"]
RUN dotnet restore "CodeMentorAI.API.csproj" --runtime linux-x64

# Copy everything else and build
COPY . .
RUN dotnet build "CodeMentorAI.API.csproj" -c Release -o /app/build --no-restore

# Publish
RUN dotnet publish "CodeMentorAI.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --runtime linux-x64 \
    --self-contained false \
    /p:PublishReadyToRun=false \
    /p:PublishSingleFile=false

# Final stage - use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Run as non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

# Expose port (will be set by Render via PORT env var)
EXPOSE 8080

ENTRYPOINT ["dotnet", "CodeMentorAI.API.dll"]
