namespace CodeMentorAI.API.DTOs;

public class DashboardStatsDto
{
    public UserStatsDto User { get; set; } = null!;
    public List<AchievementDto> RecentAchievements { get; set; } = new();
    public List<AchievementProgressDto> AvailableAchievements { get; set; } = new();
    public List<ActivityDto> RecentActivity { get; set; } = new();
}

public class UserStatsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Xp { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
    public int Streak { get; set; }
}

public class AchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public string? UnlockedAt { get; set; }
}

public class AchievementProgressDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string RequirementType { get; set; } = string.Empty;
    public int RequirementValue { get; set; }
    public int CurrentProgress { get; set; }
    public int ProgressPercentage { get; set; }
}

public class ActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public int XpEarned { get; set; }
}

public class ActivityUpdateDto
{
    public string Type { get; set; } = string.Empty; // bug_fixed, game_won, code_analyzed
    public int Value { get; set; } = 1;
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Xp { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
}