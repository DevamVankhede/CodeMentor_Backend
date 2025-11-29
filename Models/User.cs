using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeMentorAI.API.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string? ProfilePictureUrl { get; set; }
    
    public bool IsEmailVerified { get; set; } = false;
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsAdmin { get; set; } = false;
    
    // Navigation properties
    public virtual UserProfile? Profile { get; set; }
    public virtual ICollection<CodeSnippet> CodeSnippets { get; set; } = new List<CodeSnippet>();
    public virtual ICollection<GameResult> GameResults { get; set; } = new List<GameResult>();
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    public virtual ICollection<CollaborationSession> OwnedSessions { get; set; } = new List<CollaborationSession>();
    public virtual ICollection<CollaborationParticipant> SessionParticipations { get; set; } = new List<CollaborationParticipant>();
}

[Table("User_Profiles")]
public class UserProfile
{
    [Key]
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    public int Level { get; set; } = 1;
    
    public int XpPoints { get; set; } = 0;
    
    public int BugsFixed { get; set; } = 0;
    
    public int GamesWon { get; set; } = 0;
    
    public int CurrentStreak { get; set; } = 0;
    
    public string? PreferredLanguages { get; set; }
    
    public string? LearningGoals { get; set; }
    
    public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}

[Table("Code_Snippets")]
public class CodeSnippet
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Language { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsPublic { get; set; } = false;
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    public string? Tags { get; set; }
    
    public int ViewCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<CodeAnalysis> Analyses { get; set; } = new List<CodeAnalysis>();
}

[Table("Code_Analyses")]
public class CodeAnalysis
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("CodeSnippet")]
    public int CodeSnippetId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string AnalysisType { get; set; } = string.Empty;
    
    [Required]
    public string Result { get; set; } = string.Empty;
    
    public decimal? Confidence { get; set; }
    
    public int? ProcessingTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual CodeSnippet CodeSnippet { get; set; } = null!;
}

[Table("Achievements")]
public class Achievement
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Icon { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Rarity { get; set; } = string.Empty; // common, rare, epic, legendary
    
    public int XpReward { get; set; } = 0;
    
    [Required]
    [MaxLength(50)]
    public string RequirementType { get; set; } = string.Empty;
    
    public int RequirementValue { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}

[Table("User_Achievements")]
public class UserAchievement
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [ForeignKey("Achievement")]
    public int AchievementId { get; set; }
    
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    
    public int Progress { get; set; } = 0;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Achievement Achievement { get; set; } = null!;
}