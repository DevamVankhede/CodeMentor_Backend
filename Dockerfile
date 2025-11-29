# Multi-stage build for .NET 8.0 API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CodeMentorAI.API.csproj", "./"]
RUN dotnet restore "CodeMentorAI.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "CodeMentorAI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeMentorAI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CodeMentorAI.API.dll"]
