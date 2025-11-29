using System.ComponentModel.DataAnnotations;

namespace CodeMentorAI.API.Models;

public class GameResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string GameType { get; set; } = string.Empty; // bug-hunt, code-completion, refactor-challenge
    public int Score { get; set; }
    public int TimeSpent { get; set; } // in seconds
    public string Difficulty { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty; // JSON with game-specific data
    public int XpEarned { get; set; } = 0;
    public DateTime CompletedAt { get; set; }
}

public class CollaborationSession
{
    public int Id { get; set; }
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<CollaborationParticipant> Participants { get; set; } = new List<CollaborationParticipant>();
}

public class CollaborationParticipant
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public CollaborationSession Session { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AIInteraction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // analyze, explain, refactor, bug-find
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LearningRoadmap
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON roadmap data
    public string Status { get; set; } = "active"; // active, completed, paused
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<RoadmapMilestone> Milestones { get; set; } = new List<RoadmapMilestone>();
}

public class RoadmapMilestone
{
    public int Id { get; set; }
    public int RoadmapId { get; set; }
    public LearningRoadmap Roadmap { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public string Topics { get; set; } = string.Empty; // JSON array
    public string Projects { get; set; } = string.Empty; // JSON array
    public string Resources { get; set; } = string.Empty; // JSON array
}