using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeMentorAI.API.Models;

[Table("Roadmaps")]
public class Roadmap
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Difficulty { get; set; } = string.Empty; // beginner, intermediate, advanced
    
    [Required]
    [MaxLength(50)]
    public string EstimatedDuration { get; set; } = string.Empty; // e.g., "6 months", "3 weeks"
    
    public string Topics { get; set; } = string.Empty; // JSON array of topics
    
    public string Goals { get; set; } = string.Empty; // JSON array of learning goals
    
    [ForeignKey("User")]
    public int AuthorId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active"; // active, draft, archived
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<RoadmapEnrollment> Enrollments { get; set; } = new List<RoadmapEnrollment>();
    public virtual ICollection<RoadmapStep> Steps { get; set; } = new List<RoadmapStep>();
}

[Table("Roadmap_Enrollments")]
public class RoadmapEnrollment
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("Roadmap")]
    public int RoadmapId { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    public int Progress { get; set; } = 0; // Percentage (0-100)
    
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    
    public string? Notes { get; set; } // User's personal notes
    
    // Navigation properties
    public virtual Roadmap Roadmap { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

[Table("Roadmap_Steps")]
public class RoadmapStep
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("Roadmap")]
    public int RoadmapId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public int OrderIndex { get; set; } // For ordering steps
    
    [MaxLength(50)]
    public string StepType { get; set; } = "learning"; // learning, project, assessment
    
    public string? Resources { get; set; } // JSON array of resources (links, books, etc.)
    
    public string? Requirements { get; set; } // JSON array of prerequisites
    
    public int EstimatedHours { get; set; } = 0;
    
    public bool IsOptional { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Roadmap Roadmap { get; set; } = null!;
    public virtual ICollection<UserStepProgress> UserProgress { get; set; } = new List<UserStepProgress>();
}

[Table("User_Step_Progress")]
public class UserStepProgress
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("RoadmapStep")]
    public int StepId { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    public int Progress { get; set; } = 0; // Percentage (0-100)
    
    public string? Notes { get; set; } // User's notes for this step
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual RoadmapStep Step { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

[Table("Custom_Editors")]
public class CustomEditor
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [ForeignKey("User")]
    public int AuthorId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Language { get; set; } = string.Empty;
    
    public string Settings { get; set; } = string.Empty; // JSON editor settings
    
    public string Features { get; set; } = string.Empty; // JSON enabled features
    
    public string DefaultCode { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
    
    public bool IsTemplate { get; set; } = false;
    
    public int UsageCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User Author { get; set; } = null!;
}