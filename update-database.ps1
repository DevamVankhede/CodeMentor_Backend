# PowerShell script to update database while API is running
Write-Host "🔄 Updating database with new roadmap models..." -ForegroundColor Yellow

try {
    # Add migration
    Write-Host "📝 Creating migration..." -ForegroundColor Cyan
    dotnet ef migrations add AddRoadmapModels --force
    
    # Update database
    Write-Host "🗄️ Updating database..." -ForegroundColor Cyan
    dotnet ef database update
    
    Write-Host "✅ Database updated successfully!" -ForegroundColor Green
    Write-Host "🚀 The API should now support roadmap endpoints." -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error updating database: $_" -ForegroundColor Red
    Write-Host "💡 You may need to stop the API first with: Stop-Process -Name 'CodeMentorAI.API' -Force" -ForegroundColor Yellow
}

Write-Host "Checking if roadmap tables exist..." -ForegroundColor Cyan
# You can manually check the database file or run a query to verify tables exist