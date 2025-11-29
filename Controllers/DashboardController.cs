using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.DTOs;

namespace CodeMentorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserAchievements)
                    .ThenInclude(ua => ua.Achievement)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            // Get recent achievements
            var recentAchievements = user.UserAchievements
                .OrderByDescending(ua => ua.UnlockedAt)
                .Take(3)
                .Select(ua => new AchievementDto
                {
                    Id = ua.Achievement.Id,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    Icon = ua.Achievement.Icon,
                    Rarity = ua.Achievement.Rarity,
                    XpReward = ua.Achievement.XpReward,
                    UnlockedAt = ua.UnlockedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToList();

            // Get available achievements (not unlocked yet)
            var unlockedAchievementIds = user.UserAchievements.Select(ua => ua.AchievementId).ToHashSet();
            var availableAchievements = await _context.Achievements
                .Where(a => !unlockedAchievementIds.Contains(a.Id))
                .Take(3)
                .Select(a => new AchievementProgressDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Icon = a.Icon,
                    Rarity = a.Rarity,
                    RequirementType = a.RequirementType,
                    RequirementValue = a.RequirementValue,
                    CurrentProgress = GetProgressForRequirement(user, a.RequirementType),
                    ProgressPercentage = CalculateProgressPercentage(user, a.RequirementType, a.RequirementValue)
                })
                .ToListAsync();

            return Ok(new DashboardStatsDto
            {
                User = new UserStatsDto
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Level = user.Profile?.Level ?? 1,
                    Xp = user.Profile?.XpPoints ?? 0,
                    BugsFixed = user.Profile?.BugsFixed ?? 0,
                    GamesWon = user.Profile?.GamesWon ?? 0,
                    Streak = user.Profile?.CurrentStreak ?? 0
                },
                RecentAchievements = recentAchievements,
                AvailableAchievements = availableAchievements,
                RecentActivity = await GetRecentActivity(userId.Value)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return StatusCode(500, new { message = "Failed to get dashboard data" });
        }
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard()
    {
        try
        {
            var leaderboard = await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.Profile != null)
                .OrderByDescending(u => u.Profile!.XpPoints)
                .Take(10)
                .Select(u => new LeaderboardEntryDto
                {
                    Rank = 0, // Will be set after ordering
                    Name = u.Name,
                    Level = u.Profile!.Level,
                    Xp = u.Profile.XpPoints,
                    BugsFixed = u.Profile.BugsFixed,
                    GamesWon = u.Profile.GamesWon
                })
                .ToListAsync();

            // Set ranks
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, new { message = "Failed to get leaderboard" });
        }
    }

    [HttpPost("update-activity")]
    public async Task<ActionResult> UpdateActivity([FromBody] ActivityUpdateDto activity)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Profile == null) return NotFound();

            // Update profile based on activity
            switch (activity.Type.ToLower())
            {
                case "bug_fixed":
                    user.Profile.BugsFixed += activity.Value;
                    user.Profile.XpPoints += 10; // 10 XP per bug fixed
                    break;
                case "game_won":
                    user.Profile.GamesWon += activity.Value;
                    user.Profile.XpPoints += 25; // 25 XP per game won
                    break;
                case "code_analyzed":
                    user.Profile.XpPoints += 5; // 5 XP per analysis
                    break;
            }

            // Update level based on XP
            var newLevel = CalculateLevel(user.Profile.XpPoints);
            if (newLevel > user.Profile.Level)
            {
                user.Profile.Level = newLevel;
                // Award level-up XP bonus
                user.Profile.XpPoints += newLevel * 10;
            }

            user.Profile.LastActiveDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Check for new achievements
            await CheckAndUnlockAchievements(userId.Value);

            return Ok(new { message = "Activity updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity");
            return StatusCode(500, new { message = "Failed to update activity" });
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

    private int GetProgressForRequirement(Models.User user, string requirementType)
    {
        return requirementType.ToLower() switch
        {
            "bugs_fixed" => user.Profile?.BugsFixed ?? 0,
            "games_won" => user.Profile?.GamesWon ?? 0,
            "level" => user.Profile?.Level ?? 1,
            "streak" => user.Profile?.CurrentStreak ?? 0,
            _ => 0
        };
    }

    private int CalculateProgressPercentage(Models.User user, string requirementType, int requirementValue)
    {
        var current = GetProgressForRequirement(user, requirementType);
        return Math.Min(100, (int)((double)current / requirementValue * 100));
    }

    private async Task<List<ActivityDto>> GetRecentActivity(int userId)
    {
        var activities = new List<ActivityDto>();

        // Get recent game results
        var recentGames = await _context.GameResults
            .Where(gr => gr.UserId == userId)
            .OrderByDescending(gr => gr.CompletedAt)
            .Take(5)
            .ToListAsync();

        activities.AddRange(recentGames.Select(gr => new ActivityDto
        {
            Type = "game",
            Description = $"Completed {gr.GameType} game with score {gr.Score}",
            Timestamp = gr.CompletedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            XpEarned = gr.XpEarned
        }));

        return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private int CalculateLevel(int xp)
    {
        // Simple level calculation: every 100 XP = 1 level
        return Math.Max(1, xp / 100 + 1);
    }

    private async Task CheckAndUnlockAchievements(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.UserAchievements)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Profile == null) return;

        var unlockedAchievementIds = user.UserAchievements.Select(ua => ua.AchievementId).ToHashSet();
        var availableAchievements = await _context.Achievements
            .Where(a => !unlockedAchievementIds.Contains(a.Id))
            .ToListAsync();

        foreach (var achievement in availableAchievements)
        {
            var currentProgress = GetProgressForRequirement(user, achievement.RequirementType);
            if (currentProgress >= achievement.RequirementValue)
            {
                // Unlock achievement
                var userAchievement = new Models.UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    UnlockedAt = DateTime.UtcNow,
                    Progress = currentProgress
                };

                _context.UserAchievements.Add(userAchievement);
                
                // Award XP
                user.Profile.XpPoints += achievement.XpReward;
            }
        }

        await _context.SaveChangesAsync();
    }
}