using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeMentorAI.API.Services;
using CodeMentorAI.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodeMentorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly AppDbContext _context;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIService aiService, AppDbContext context, ILogger<AIController> logger)
    {
        _aiService = aiService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("analyze-code")]
    public async Task<IActionResult> AnalyzeCode([FromBody] AnalyzeCodeRequest request)
    {
        try
        {
            var suggestions = await _aiService.GenerateCodeSuggestion(request.Code, request.Language, request.Context);
            return Ok(new { success = true, suggestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code");
            return StatusCode(500, new { message = "Failed to analyze code" });
        }
    }

    [HttpPost("find-bugs")]
    public async Task<IActionResult> FindBugs([FromBody] FindBugsRequest request)
    {
        try
        {
            var bugs = await _aiService.FindBugs(request.Code, request.Language);
            return Ok(new { success = true, bugs });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding bugs");
            return StatusCode(500, new { message = "Failed to find bugs" });
        }
    }

    [HttpPost("explain-code")]
    public async Task<IActionResult> ExplainCode([FromBody] ExplainCodeRequest request)
    {
        try
        {
            var explanation = await _aiService.ExplainCode(request.Code, request.Language, request.Level);
            return Ok(new { success = true, explanation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining code");
            return StatusCode(500, new { message = "Failed to explain code" });
        }
    }

    [HttpPost("generate-roadmap")]
    public async Task<IActionResult> GenerateRoadmap()
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Profile == null) return NotFound("User profile not found");

            var preferredLanguages = System.Text.Json.JsonSerializer.Deserialize<string[]>(
                user.Profile.PreferredLanguages ?? "[]") ?? new[] { "JavaScript" };

            var userLevel = user.Profile.Level switch
            {
                <= 5 => "beginner",
                <= 15 => "intermediate",
                _ => "advanced"
            };

            var roadmap = await _aiService.GenerateRoadmap(userLevel, preferredLanguages, user.Profile.LearningGoals ?? "General programming improvement");
            
            return Ok(new { success = true, roadmap });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating roadmap");
            return StatusCode(500, new { message = "Failed to generate roadmap" });
        }
    }

    [HttpPost("refactor-code")]
    public async Task<IActionResult> RefactorCode([FromBody] RefactorCodeRequest request)
    {
        try
        {
            var refactored = await _aiService.RefactorCode(request.Code, request.Language);
            return Ok(new { success = true, refactored });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refactoring code");
            return StatusCode(500, new { message = "Failed to refactor code" });
        }
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }
}

// Request DTOs
public class AnalyzeCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public class FindBugsRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

public class ExplainCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Level { get; set; } = "intermediate";
}

public class RefactorCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}