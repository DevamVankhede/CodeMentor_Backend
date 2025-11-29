using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.Models;
using CodeMentorAI.API.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace CodeMentorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<GameController> _logger;

    public GameController(AppDbContext context, ILogger<GameController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("submit-result")]
    public async Task<ActionResult<GameResultResponse>> SubmitGameResult([FromBody] GameResultRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            // Calculate XP based on score, difficulty, and time
            var xpEarned = CalculateXpEarned(request.Score, request.Difficulty, request.TimeSpent);

            // Create game result
            var gameResult = new GameResult
            {
                UserId = userId.Value,
                GameType = request.GameType,
                Score = request.Score,
                TimeSpent = request.TimeSpent,
                Difficulty = request.Difficulty,
                Details = JsonSerializer.Serialize(request.Details),
                XpEarned = xpEarned,
                CompletedAt = DateTime.UtcNow
            };

            _context.GameResults.Add(gameResult);

            // Update user profile
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (userProfile != null)
            {
                userProfile.XpPoints += xpEarned;
                
                // Update level based on XP
                var newLevel = CalculateLevel(userProfile.XpPoints);
                if (newLevel > userProfile.Level)
                {
                    userProfile.Level = newLevel;
                }

                // Update game-specific stats
                if (request.GameType == "bug-hunt" && request.Score > 0)
                {
                    userProfile.BugsFixed += 1;
                }

                if (request.Score >= 80) // Consider 80+ score as a win
                {
                    userProfile.GamesWon += 1;
                }

                // Update streak (simplified - just increment for now)
                userProfile.CurrentStreak += 1;
                userProfile.LastActiveDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Get updated user profile
            var updatedProfile = await _context.UserProfiles
                .FirstAsync(p => p.UserId == userId.Value);

            return Ok(new GameResultResponse
            {
                XpEarned = xpEarned,
                NewLevel = updatedProfile.Level,
                TotalXp = updatedProfile.XpPoints,
                BugsFixed = updatedProfile.BugsFixed,
                GamesWon = updatedProfile.GamesWon,
                CurrentStreak = updatedProfile.CurrentStreak,
                LevelUp = updatedProfile.Level > (userProfile?.Level ?? 1)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting game result");
            return StatusCode(500, new { message = "Failed to submit game result" });
        }
    }

    [HttpGet("user-stats")]
    public async Task<ActionResult<UserGameStats>> GetUserStats()
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (userProfile == null)
            {
                return NotFound(new { message = "User profile not found" });
            }

            // Get recent game results
            var recentResults = await _context.GameResults
                .Where(gr => gr.UserId == userId.Value)
                .OrderByDescending(gr => gr.CompletedAt)
                .Take(10)
                .ToListAsync();

            // Calculate difficulty progression
            var easyGames = recentResults.Count(gr => gr.Difficulty == "easy");
            var mediumGames = recentResults.Count(gr => gr.Difficulty == "medium");
            var hardGames = recentResults.Count(gr => gr.Difficulty == "hard");

            var recommendedDifficulty = DetermineRecommendedDifficulty(easyGames, mediumGames, hardGames);

            return Ok(new UserGameStats
            {
                Level = userProfile.Level,
                XpPoints = userProfile.XpPoints,
                BugsFixed = userProfile.BugsFixed,
                GamesWon = userProfile.GamesWon,
                CurrentStreak = userProfile.CurrentStreak,
                RecommendedDifficulty = recommendedDifficulty,
                RecentResults = recentResults.Select(gr => new GameResultSummary
                {
                    GameType = gr.GameType,
                    Score = gr.Score,
                    Difficulty = gr.Difficulty,
                    XpEarned = gr.XpEarned,
                    CompletedAt = gr.CompletedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user game stats");
            return StatusCode(500, new { message = "Failed to fetch user stats" });
        }
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard()
    {
        try
        {
            var leaderboard = await _context.UserProfiles
                .Include(p => p.User)
                .OrderByDescending(p => p.XpPoints)
                .Take(10)
                .Select(p => new LeaderboardEntry
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    Level = p.Level,
                    XpPoints = p.XpPoints,
                    BugsFixed = p.BugsFixed,
                    GamesWon = p.GamesWon,
                    CurrentStreak = p.CurrentStreak
                })
                .ToListAsync();

            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leaderboard");
            return StatusCode(500, new { message = "Failed to fetch leaderboard" });
        }
    }

    private int CalculateXpEarned(int score, string difficulty, int timeSpent)
    {
        var baseXp = score / 10; // Base XP from score
        
        // Difficulty multiplier
        var difficultyMultiplier = difficulty switch
        {
            "easy" => 1.0,
            "medium" => 1.5,
            "hard" => 2.0,
            _ => 1.0
        };

        // Time bonus (faster completion = more XP)
        var timeBonus = Math.Max(0, (300 - timeSpent) / 30); // 1 XP per 30 seconds saved

        return (int)((baseXp + timeBonus) * difficultyMultiplier);
    }

    private int CalculateLevel(int xpPoints)
    {
        // Level calculation: each level requires more XP
        // Level 1: 0-99 XP, Level 2: 100-249 XP, Level 3: 250-449 XP, etc.
        return (int)(Math.Sqrt(xpPoints / 50) + 1);
    }

    private string DetermineRecommendedDifficulty(int easyGames, int mediumGames, int hardGames)
    {
        // Simple progression logic
        if (easyGames < 5)
            return "easy";
        else if (mediumGames < 5)
            return "medium";
        else
            return "hard";
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

public class GameResultRequest
{
    public string GameType { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TimeSpent { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public object Details { get; set; } = new();
}

public class GameResultResponse
{
    public int XpEarned { get; set; }
    public int NewLevel { get; set; }
    public int TotalXp { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
    public int CurrentStreak { get; set; }
    public bool LevelUp { get; set; }
}

public class UserGameStats
{
    public int Level { get; set; }
    public int XpPoints { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
    public int CurrentStreak { get; set; }
    public string RecommendedDifficulty { get; set; } = string.Empty;
    public List<GameResultSummary> RecentResults { get; set; } = new();
}

public class GameResultSummary
{
    public string GameType { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int XpEarned { get; set; }
    public string CompletedAt { get; set; } = string.Empty;
}

public class LeaderboardEntry
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int XpPoints { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
    public int CurrentStreak { get; set; }
}