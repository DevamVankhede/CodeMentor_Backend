using CodeMentorAI.API.Data;
using CodeMentorAI.API.Models;
using System.Text.Json;

namespace CodeMentorAI.API.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
        {
            return; // Database has been seeded
        }

        // Seed Achievements
        var achievements = new[]
        {
            new Achievement
            {
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
                Name = "Legendary Coder",
                Description = "Maintain a 30-day coding streak",
                Icon = "🔥",
                Rarity = "legendary",
                XpReward = 1000,
                RequirementType = "streak",
                RequirementValue = 30
            }
        };

        context.Achievements.AddRange(achievements);
        await context.SaveChangesAsync();

        // Seed Demo Users
        var users = new[]
        {
            new User
            {
                Name = "Admin User",
                Email = "admin@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsEmailVerified = true,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Alex CodeMaster",
                Email = "alex@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("alex123"),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Sarah DevQueen",
                Email = "sarah@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("sarah123"),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Seed User Profiles
        var profiles = new[]
        {
            new UserProfile
            {
                UserId = users[0].Id,
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
                UserId = users[1].Id,
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
                UserId = users[2].Id,
                Level = 18,
                XpPoints = 3256,
                BugsFixed = 156,
                GamesWon = 62,
                CurrentStreak = 5,
                PreferredLanguages = JsonSerializer.Serialize(new[] { "Python", "Django", "PostgreSQL" }),
                LearningGoals = "Specialize in backend development and databases",
                LastActiveDate = DateTime.UtcNow.AddHours(-1)
            }
        };

        context.UserProfiles.AddRange(profiles);
        await context.SaveChangesAsync();

        // Seed Code Snippets
        var codeSnippets = new[]
        {
            new CodeSnippet
            {
                Title = "React Component Example",
                Code = "import React from 'react';\n\nconst MyComponent = () => {\n  return (\n    <div>\n      <h1>Hello World</h1>\n    </div>\n  );\n};\n\nexport default MyComponent;",
                Language = "javascript",
                Description = "A simple React functional component",
                IsPublic = true,
                UserId = users[0].Id,
                Tags = JsonSerializer.Serialize(new[] { "react", "component", "frontend" }),
                ViewCount = 45,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new CodeSnippet
            {
                Title = "Python Data Processing",
                Code = "import pandas as pd\nimport numpy as np\n\ndef process_data(df):\n    # Clean and process data\n    df_clean = df.dropna()\n    df_clean['processed'] = df_clean['value'] * 2\n    return df_clean\n\n# Usage\ndata = pd.DataFrame({'value': [1, 2, 3, None, 5]})\nresult = process_data(data)\nprint(result)",
                Language = "python",
                Description = "Data processing with pandas",
                IsPublic = true,
                UserId = users[1].Id,
                Tags = JsonSerializer.Serialize(new[] { "python", "pandas", "data-science" }),
                ViewCount = 32,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        context.CodeSnippets.AddRange(codeSnippets);
        await context.SaveChangesAsync();
    }
}