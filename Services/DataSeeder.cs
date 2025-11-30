using CodeMentorAI.API.Data;
using CodeMentorAI.API.Models;
using System.Text.Json;

namespace CodeMentorAI.API.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if data already exists - only seed if database is empty
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

        var random = new Random();
        var users = new List<User>();
        var profiles = new List<UserProfile>();

        // Seed 2 Admin Users
        var adminUsers = new[]
        {
            new User
            {
                Name = "Super Admin",
                Email = "admin@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsEmailVerified = true,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddHours(-1)
            },
            new User
            {
                Name = "Admin Manager",
                Email = "admin2@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsEmailVerified = true,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        users.AddRange(adminUsers);
        context.Users.AddRange(adminUsers);
        await context.SaveChangesAsync();

        // Create profiles for admin users
        profiles.Add(new UserProfile
        {
            UserId = adminUsers[0].Id,
            Level = 25,
            XpPoints = 5000,
            BugsFixed = 250,
            GamesWon = 100,
            CurrentStreak = 15,
            PreferredLanguages = JsonSerializer.Serialize(new[] { "JavaScript", "Python", "TypeScript", "C#" }),
            LearningGoals = "Manage platform and help developers grow",
            LastActiveDate = DateTime.UtcNow.AddHours(-1)
        });

        profiles.Add(new UserProfile
        {
            UserId = adminUsers[1].Id,
            Level = 20,
            XpPoints = 4000,
            BugsFixed = 180,
            GamesWon = 75,
            CurrentStreak = 10,
            PreferredLanguages = JsonSerializer.Serialize(new[] { "JavaScript", "React", "Node.js" }),
            LearningGoals = "Platform administration and user support",
            LastActiveDate = DateTime.UtcNow.AddHours(-2)
        });

        // Seed 1000 Dummy Users
        var firstNames = new[] { "Alex", "Jordan", "Taylor", "Morgan", "Casey", "Riley", "Avery", "Quinn", "Sage", "River", "Skyler", "Phoenix", "Blake", "Cameron", "Dakota", "Emery", "Finley", "Harper", "Hayden", "Kai" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee" };
        var domains = new[] { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "example.com", "test.com", "dev.com", "code.com" };
        var languages = new[] { "JavaScript", "Python", "Java", "TypeScript", "C#", "C++", "Go", "Rust", "Ruby", "PHP" };

        for (int i = 0; i < 1000; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var domain = domains[random.Next(domains.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}.{i}@{domain}";
            var name = $"{firstName} {lastName}";
            var createdAt = DateTime.UtcNow.AddDays(-random.Next(1, 365));
            var lastLogin = random.Next(100) < 70 ? DateTime.UtcNow.AddHours(-random.Next(1, 720)) : (DateTime?)null;

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                IsEmailVerified = random.Next(100) < 90, // 90% verified
                IsAdmin = false,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddDays(random.Next(1, 30)),
                LastLoginAt = lastLogin
            };

            users.Add(user);
            context.Users.Add(user);

            // Batch save every 100 users for performance
            if ((i + 1) % 100 == 0)
            {
                await context.SaveChangesAsync();
            }

            // Create profile for this user
            var userLevel = random.Next(1, 30);
            var xpPoints = userLevel * 100 + random.Next(0, 100);
            var bugsFixed = random.Next(0, 200);
            var gamesWon = random.Next(0, 100);
            var streak = random.Next(0, 30);
            var selectedLanguages = languages.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToArray();

            profiles.Add(new UserProfile
            {
                UserId = user.Id,
                Level = userLevel,
                XpPoints = xpPoints,
                BugsFixed = bugsFixed,
                GamesWon = gamesWon,
                CurrentStreak = streak,
                PreferredLanguages = JsonSerializer.Serialize(selectedLanguages),
                LearningGoals = $"Master {selectedLanguages[0]} and become a better developer",
                LastActiveDate = lastLogin ?? createdAt
            });
        }

        // Final save for remaining users
        await context.SaveChangesAsync();

        // Save all profiles
        context.UserProfiles.AddRange(profiles);
        await context.SaveChangesAsync();

        // Seed some Game Results for variety
        var gameTypes = new[] { "bug-hunt", "code-completion", "refactor-challenge", "speed-coding" };
        var difficulties = new[] { "easy", "medium", "hard" };

        var gameResults = new List<GameResult>();
        var selectedUsers = users.Where(u => !u.IsAdmin).OrderBy(x => random.Next()).Take(500).ToList();

        foreach (var user in selectedUsers)
        {
            var numGames = random.Next(1, 10);
            for (int i = 0; i < numGames; i++)
            {
                gameResults.Add(new GameResult
                {
                    UserId = user.Id,
                    GameType = gameTypes[random.Next(gameTypes.Length)],
                    Difficulty = difficulties[random.Next(difficulties.Length)],
                    Score = random.Next(50, 100),
                    TimeSpent = random.Next(60, 600), // 1-10 minutes
                    CompletedAt = DateTime.UtcNow.AddDays(-random.Next(0, 90)),
                    Details = JsonSerializer.Serialize(new { language = languages[random.Next(languages.Length)] })
                });
            }
        }

        context.GameResults.AddRange(gameResults);
        await context.SaveChangesAsync();

        // Seed some Code Snippets
        var codeSnippets = new List<CodeSnippet>();
        var snippetUsers = users.Where(u => !u.IsAdmin).OrderBy(x => random.Next()).Take(200).ToList();

        foreach (var user in snippetUsers)
        {
            var numSnippets = random.Next(1, 5);
            for (int i = 0; i < numSnippets; i++)
            {
                var language = languages[random.Next(languages.Length)];
                codeSnippets.Add(new CodeSnippet
                {
                    Title = $"{language} Code Example {i + 1}",
                    Code = $"// {language} code example\nfunction example() {{\n  return 'Hello World';\n}}",
                    Language = language.ToLower(),
                    Description = $"A sample {language} code snippet",
                    IsPublic = random.Next(100) < 70, // 70% public
                    UserId = user.Id,
                    Tags = JsonSerializer.Serialize(new[] { language.ToLower(), "example", "sample" }),
                    ViewCount = random.Next(0, 1000),
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 180)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                });
            }
        }

        context.CodeSnippets.AddRange(codeSnippets);
        await context.SaveChangesAsync();

        // Seed some Collaboration Sessions
        var sessions = new List<CollaborationSession>();
        var sessionOwners = users.Where(u => !u.IsAdmin).OrderBy(x => random.Next()).Take(50).ToList();

        foreach (var owner in sessionOwners)
        {
            sessions.Add(new CollaborationSession
            {
                OwnerId = owner.Id,
                RoomId = Guid.NewGuid().ToString(),
                Name = $"Collaboration Session {sessions.Count + 1}",
                Description = $"A collaborative coding session for {languages[random.Next(languages.Length)]}",
                Language = languages[random.Next(languages.Length)].ToLower(),
                Code = $"// Welcome to collaboration session\nfunction hello() {{\n  console.log('Hello from {owner.Name}');\n}}",
                IsActive = random.Next(100) < 30, // 30% active
                IsPublic = random.Next(100) < 50, // 50% public
                Status = "active",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
                UpdatedAt = DateTime.UtcNow.AddHours(-random.Next(0, 24))
            });
        }

        context.CollaborationSessions.AddRange(sessions);
        await context.SaveChangesAsync();
    }
}
