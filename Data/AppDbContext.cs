using Microsoft.EntityFrameworkCore;
using CodeMentorAI.API.Models;
using System.Text.Json;

namespace CodeMentorAI.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<CodeSnippet> CodeSnippets { get; set; }
    public DbSet<CodeAnalysis> CodeAnalyses { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }
    public DbSet<GameResult> GameResults { get; set; }
    public DbSet<CollaborationSession> CollaborationSessions { get; set; }
    public DbSet<CollaborationParticipant> CollaborationParticipants { get; set; }
    public DbSet<AIInteraction> AIInteractions { get; set; }
    public DbSet<LearningRoadmap> LearningRoadmaps { get; set; }
    public DbSet<RoadmapMilestone> RoadmapMilestones { get; set; }
    public DbSet<Roadmap> Roadmaps { get; set; }
    public DbSet<RoadmapEnrollment> RoadmapEnrollments { get; set; }
    public DbSet<RoadmapStep> RoadmapSteps { get; set; }
    public DbSet<UserStepProgress> UserStepProgress { get; set; }
    public DbSet<CustomEditor> CustomEditors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256);
            
            // One-to-one relationship with UserProfile
            entity.HasOne(u => u.Profile)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(p => p.UserId);
        });

        // CodeSnippet configuration
        modelBuilder.Entity<CodeSnippet>(entity =>
        {
            entity.HasKey(cs => cs.Id);
            entity.HasOne(cs => cs.User)
                  .WithMany(u => u.CodeSnippets)
                  .HasForeignKey(cs => cs.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(cs => cs.Language);
            entity.HasIndex(cs => cs.CreatedAt);
            entity.HasIndex(cs => cs.IsPublic);
        });

        // CodeAnalysis configuration
        modelBuilder.Entity<CodeAnalysis>(entity =>
        {
            entity.HasKey(ca => ca.Id);
            entity.HasOne(ca => ca.CodeSnippet)
                  .WithMany(cs => cs.Analyses)
                  .HasForeignKey(ca => ca.CodeSnippetId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ca => ca.AnalysisType);
            entity.HasIndex(ca => ca.CreatedAt);
        });

        // Achievement configuration
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Name).IsUnique();
        });

        // UserAchievement configuration
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(ua => ua.Id);
            entity.HasOne(ua => ua.User)
                  .WithMany(u => u.UserAchievements)
                  .HasForeignKey(ua => ua.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ua => ua.Achievement)
                  .WithMany(a => a.UserAchievements)
                  .HasForeignKey(ua => ua.AchievementId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ua => ua.UnlockedAt);
        });

        // GameResult configuration
        modelBuilder.Entity<GameResult>(entity =>
        {
            entity.HasKey(gr => gr.Id);
            entity.HasOne(gr => gr.User)
                  .WithMany(u => u.GameResults)
                  .HasForeignKey(gr => gr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(gr => gr.GameType);
            entity.HasIndex(gr => gr.CompletedAt);
        });

        // CollaborationSession configuration
        modelBuilder.Entity<CollaborationSession>(entity =>
        {
            entity.HasKey(cs => cs.Id);
            entity.HasOne(cs => cs.Owner)
                  .WithMany(u => u.OwnedSessions)
                  .HasForeignKey(cs => cs.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(cs => cs.Status);
            entity.HasIndex(cs => cs.IsPublic);
        });

        // CollaborationParticipant configuration
        modelBuilder.Entity<CollaborationParticipant>(entity =>
        {
            entity.HasKey(cp => cp.Id);
            entity.HasOne(cp => cp.Session)
                  .WithMany(cs => cs.Participants)
                  .HasForeignKey(cp => cp.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cp => cp.User)
                  .WithMany(u => u.SessionParticipations)
                  .HasForeignKey(cp => cp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(cp => cp.IsActive);
        });

        // Roadmap configuration
        modelBuilder.Entity<Roadmap>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne(r => r.Author)
                  .WithMany()
                  .HasForeignKey(r => r.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(r => r.Category);
            entity.HasIndex(r => r.Difficulty);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.CreatedAt);
        });

        // RoadmapEnrollment configuration
        modelBuilder.Entity<RoadmapEnrollment>(entity =>
        {
            entity.HasKey(re => re.Id);
            entity.HasOne(re => re.Roadmap)
                  .WithMany(r => r.Enrollments)
                  .HasForeignKey(re => re.RoadmapId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(re => re.User)
                  .WithMany()
                  .HasForeignKey(re => re.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(re => re.EnrolledAt);
            entity.HasIndex(re => re.Progress);
        });

        // RoadmapStep configuration
        modelBuilder.Entity<RoadmapStep>(entity =>
        {
            entity.HasKey(rs => rs.Id);
            entity.HasOne(rs => rs.Roadmap)
                  .WithMany(r => r.Steps)
                  .HasForeignKey(rs => rs.RoadmapId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(rs => rs.OrderIndex);
            entity.HasIndex(rs => rs.StepType);
        });

        // UserStepProgress configuration
        modelBuilder.Entity<UserStepProgress>(entity =>
        {
            entity.HasKey(usp => usp.Id);
            entity.HasOne(usp => usp.Step)
                  .WithMany(rs => rs.UserProgress)
                  .HasForeignKey(usp => usp.StepId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(usp => usp.User)
                  .WithMany()
                  .HasForeignKey(usp => usp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(usp => usp.IsCompleted);
            entity.HasIndex(usp => usp.Progress);
        });

        // CustomEditor configuration
        modelBuilder.Entity<CustomEditor>(entity =>
        {
            entity.HasKey(ce => ce.Id);
            entity.HasOne(ce => ce.Author)
                  .WithMany()
                  .HasForeignKey(ce => ce.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(ce => ce.Language);
            entity.HasIndex(ce => ce.IsPublic);
            entity.HasIndex(ce => ce.IsTemplate);
            entity.HasIndex(ce => ce.CreatedAt);
        });

        // Seed data - will be added after database creation
        // SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Achievements
        modelBuilder.Entity<Achievement>().HasData(
            new Achievement
            {
                Id = 1,
                Name = "First Steps",
                Description = "Complete your first code analysis",
                Icon = "👶",
                Rarity = "common",
                XpReward = 50,
                RequirementType = "ai_interactions",
                RequirementValue = 1
            },
            new Achievement
            {
                Id = 2,
                Name = "Bug Hunter",
                Description = "Fix 10 bugs with AI assistance",
                Icon = "🐛",
                Rarity = "rare",
                XpReward = 200,
                RequirementType = "bugs_fixed",
                RequirementValue = 10
            },
            new Achievement
            {
                Id = 3,
                Name = "Code Master",
                Description = "Reach level 10",
                Icon = "👑",
                Rarity = "epic",
                XpReward = 500,
                RequirementType = "level",
                RequirementValue = 10
            },
            new Achievement
            {
                Id = 4,
                Name = "Game Champion",
                Description = "Win 25 coding games",
                Icon = "🏆",
                Rarity = "epic",
                XpReward = 300,
                RequirementType = "games_won",
                RequirementValue = 25
            },
            new Achievement
            {
                Id = 5,
                Name = "Legendary Coder",
                Description = "Maintain a 30-day coding streak",
                Icon = "🔥",
                Rarity = "legendary",
                XpReward = 1000,
                RequirementType = "streak",
                RequirementValue = 30
            }
        );

        // Seed Demo Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Demo Warrior",
                Email = "demo@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("demo123"),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Name = "Alex CodeMaster",
                Email = "alex@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("alex123"),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                Name = "Sarah DevQueen",
                Email = "sarah@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("sarah123"),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed User Profiles
        modelBuilder.Entity<UserProfile>().HasData(
            new UserProfile
            {
                UserId = 1,
                Level = 15,
                XpPoints = 2847,
                BugsFixed = 127,
                GamesWon = 43,
                CurrentStreak = 7,
                PreferredLanguages = JsonSerializer.Serialize(new[] { "JavaScript", "Python", "TypeScript" }),
                LearningGoals = "Master full-stack development and AI integration",
                LastActiveDate = DateTime.UtcNow
            },
            new UserProfile
            {
                UserId = 2,
                Level = 22,
                XpPoints = 4521,
                BugsFixed = 203,
                GamesWon = 78,
                CurrentStreak = 12,
                PreferredLanguages = JsonSerializer.Serialize(new[] { "JavaScript", "React", "Node.js" }),
                LearningGoals = "Become a senior full-stack developer",
                LastActiveDate = DateTime.UtcNow.AddHours(-2)
            },
            new UserProfile
            {
                UserId = 3,
                Level = 18,
                XpPoints = 3256,
                BugsFixed = 156,
                GamesWon = 62,
                CurrentStreak = 5,
                PreferredLanguages = JsonSerializer.Serialize(new[] { "Python", "Django", "PostgreSQL" }),
                LearningGoals = "Specialize in backend development and databases",
                LastActiveDate = DateTime.UtcNow.AddHours(-1)
            }
        );

        // Seed Code Snippets
        modelBuilder.Entity<CodeSnippet>().HasData(
            new CodeSnippet
            {
                Id = 1,
                Title = "React Component Example",
                Code = "import React from 'react';\n\nconst MyComponent = () => {\n  return (\n    <div>\n      <h1>Hello World</h1>\n    </div>\n  );\n};\n\nexport default MyComponent;",
                Language = "javascript",
                Description = "A simple React functional component",
                IsPublic = true,
                UserId = 1,
                Tags = JsonSerializer.Serialize(new[] { "react", "component", "frontend" }),
                ViewCount = 45,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new CodeSnippet
            {
                Id = 2,
                Title = "Python Data Processing",
                Code = "import pandas as pd\nimport numpy as np\n\ndef process_data(df):\n    # Clean and process data\n    df_clean = df.dropna()\n    df_clean['processed'] = df_clean['value'] * 2\n    return df_clean\n\n# Usage\ndata = pd.DataFrame({'value': [1, 2, 3, None, 5]})\nresult = process_data(data)\nprint(result)",
                Language = "python",
                Description = "Data processing with pandas",
                IsPublic = true,
                UserId = 2,
                Tags = JsonSerializer.Serialize(new[] { "python", "pandas", "data-science" }),
                ViewCount = 32,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        );
    }
}