using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.Models;
using CodeMentorAI.API.DTOs;
using System.Security.Claims;

namespace CodeMentorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var users = await _context.Users
                .Include(u => u.Profile)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userDtos = users.Select(MapToAdminUserDto).ToList();
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users for admin");
            return StatusCode(500, new { message = "Failed to fetch users" });
        }
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<AdminUserDto>> GetUser(int id)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(MapToAdminUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user for admin");
            return StatusCode(500, new { message = "Failed to fetch user" });
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already registered" });
            }

            // Create new user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsEmailVerified = true,
                IsAdmin = request.IsAdmin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create user profile
            var profile = new UserProfile
            {
                UserId = user.Id,
                Level = 1,
                XpPoints = 0,
                BugsFixed = 0,
                GamesWon = 0,
                CurrentStreak = 0,
                PreferredLanguages = System.Text.Json.JsonSerializer.Serialize(new[] { "JavaScript" }),
                LearningGoals = "Start my coding journey",
                LastActiveDate = DateTime.UtcNow
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            // Load user with profile for response
            var userWithProfile = await _context.Users
                .Include(u => u.Profile)
                .FirstAsync(u => u.Id == user.Id);

            return Ok(MapToAdminUserDto(userWithProfile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "Failed to create user" });
        }
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user properties
            user.Name = request.Name ?? user.Name;
            user.Email = request.Email ?? user.Email;
            user.IsAdmin = request.IsAdmin ?? user.IsAdmin;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Update profile if provided
            if (user.Profile != null)
            {
                if (request.Level.HasValue)
                    user.Profile.Level = request.Level.Value;
                if (request.XpPoints.HasValue)
                    user.Profile.XpPoints = request.XpPoints.Value;
                if (request.BugsFixed.HasValue)
                    user.Profile.BugsFixed = request.BugsFixed.Value;
                if (request.GamesWon.HasValue)
                    user.Profile.GamesWon = request.GamesWon.Value;
                if (request.CurrentStreak.HasValue)
                    user.Profile.CurrentStreak = request.CurrentStreak.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(MapToAdminUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return StatusCode(500, new { message = "Failed to update user" });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return StatusCode(500, new { message = "Failed to delete user" });
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<AdminSessionDto>>> GetSessions()
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var sessions = await _context.CollaborationSessions
                .Include(s => s.Owner)
                .Include(s => s.Participants.Where(p => p.IsActive))
                    .ThenInclude(p => p.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var sessionDtos = sessions.Select(MapToAdminSessionDto).ToList();
            return Ok(sessionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sessions for admin");
            return StatusCode(500, new { message = "Failed to fetch sessions" });
        }
    }

    [HttpDelete("sessions/{id}")]
    public async Task<ActionResult> DeleteSession(int id)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var session = await _context.CollaborationSessions.FindAsync(id);
            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            _context.CollaborationSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session");
            return StatusCode(500, new { message = "Failed to delete session" });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.LastLoginAt > DateTime.UtcNow.AddDays(-30));
            var totalSessions = await _context.CollaborationSessions.CountAsync();
            var activeSessions = await _context.CollaborationSessions.CountAsync(s => s.IsActive);
            var totalGameResults = await _context.GameResults.CountAsync();
            var totalCodeSnippets = await _context.CodeSnippets.CountAsync();
            var totalRoadmaps = await _context.Roadmaps.CountAsync();
            var totalEditors = await _context.CustomEditors.CountAsync();

            // Get recent activity
            var recentActivity = new List<AdminActivityDto>();
            
            // Recent user signups
            var recentUsers = await _context.Users
                .Where(u => u.CreatedAt > DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();
            
            foreach (var user in recentUsers)
            {
                recentActivity.Add(new AdminActivityDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "user_signup",
                    User = user.Email,
                    Description = "New user registered",
                    Timestamp = user.CreatedAt
                });
            }

            // Recent roadmaps
            var recentRoadmaps = await _context.Roadmaps
                .Include(r => r.Author)
                .Where(r => r.CreatedAt > DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();
            
            foreach (var roadmap in recentRoadmaps)
            {
                recentActivity.Add(new AdminActivityDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "roadmap_created",
                    User = roadmap.Author?.Email ?? "Unknown",
                    Description = $"Created \"{roadmap.Title}\" roadmap",
                    Timestamp = roadmap.CreatedAt
                });
            }

            var stats = new AdminStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalSessions = totalSessions,
                ActiveSessions = activeSessions,
                TotalGameResults = totalGameResults,
                TotalCodeSnippets = totalCodeSnippets,
                TotalRoadmaps = totalRoadmaps,
                TotalEditors = totalEditors,
                RecentActivity = recentActivity.OrderByDescending(a => a.Timestamp).Take(10).ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin stats");
            return StatusCode(500, new { message = "Failed to fetch stats" });
        }
    }

    [HttpGet("roadmaps")]
    public async Task<ActionResult<List<AdminRoadmapDto>>> GetRoadmaps()
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var roadmaps = await _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var roadmapDtos = roadmaps.Select(MapToAdminRoadmapDto).ToList();
            return Ok(roadmapDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roadmaps for admin");
            return StatusCode(500, new { message = "Failed to fetch roadmaps" });
        }
    }

    [HttpPost("roadmaps")]
    public async Task<ActionResult<AdminRoadmapDto>> CreateRoadmap([FromBody] CreateRoadmapRequest request)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var roadmap = new Roadmap
            {
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Difficulty = request.Difficulty,
                EstimatedDuration = request.EstimatedDuration,
                Topics = System.Text.Json.JsonSerializer.Serialize(request.Topics),
                Goals = System.Text.Json.JsonSerializer.Serialize(request.Goals),
                AuthorId = userId.Value,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Roadmaps.Add(roadmap);
            await _context.SaveChangesAsync();

            // Load roadmap with author for response
            var roadmapWithAuthor = await _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .FirstAsync(r => r.Id == roadmap.Id);

            return Ok(MapToAdminRoadmapDto(roadmapWithAuthor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating roadmap");
            return StatusCode(500, new { message = "Failed to create roadmap" });
        }
    }

    [HttpPut("roadmaps/{id}")]
    public async Task<ActionResult<AdminRoadmapDto>> UpdateRoadmap(int id, [FromBody] UpdateRoadmapRequest request)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var roadmap = await _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (roadmap == null)
            {
                return NotFound(new { message = "Roadmap not found" });
            }

            // Update roadmap properties
            roadmap.Title = request.Title ?? roadmap.Title;
            roadmap.Description = request.Description ?? roadmap.Description;
            roadmap.Category = request.Category ?? roadmap.Category;
            roadmap.Difficulty = request.Difficulty ?? roadmap.Difficulty;
            roadmap.EstimatedDuration = request.EstimatedDuration ?? roadmap.EstimatedDuration;
            roadmap.Status = request.Status ?? roadmap.Status;
            
            if (request.Topics != null)
                roadmap.Topics = System.Text.Json.JsonSerializer.Serialize(request.Topics);
            if (request.Goals != null)
                roadmap.Goals = System.Text.Json.JsonSerializer.Serialize(request.Goals);
            
            roadmap.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToAdminRoadmapDto(roadmap));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roadmap");
            return StatusCode(500, new { message = "Failed to update roadmap" });
        }
    }

    [HttpDelete("roadmaps/{id}")]
    public async Task<ActionResult> DeleteRoadmap(int id)
    {
        try
        {
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var roadmap = await _context.Roadmaps.FindAsync(id);
            if (roadmap == null)
            {
                return NotFound(new { message = "Roadmap not found" });
            }

            _context.Roadmaps.Remove(roadmap);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Roadmap deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting roadmap");
            return StatusCode(500, new { message = "Failed to delete roadmap" });
        }
    }

    [HttpPost("make-admin/{email}")]
    [AllowAnonymous] // Allow this endpoint without authentication for initial setup
    public async Task<ActionResult> MakeUserAdmin(string email)
    {
        try
        {
            // This endpoint can be used without admin check for initial setup
            // In production, you should secure this or remove it after setup
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.IsAdmin = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {email} is now an admin" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making user admin");
            return StatusCode(500, new { message = "Failed to make user admin" });
        }
    }

    [HttpPost("create-admin")]
    [AllowAnonymous] // Allow this endpoint without authentication for initial setup
    public async Task<ActionResult> CreateAdminUser()
    {
        try
        {
            // Check if admin user already exists
            var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@codementor.ai");
            if (existingAdmin != null)
            {
                existingAdmin.IsAdmin = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Admin user already exists and admin status confirmed" });
            }

            // Create new admin user
            var adminUser = new User
            {
                Name = "Admin User",
                Email = "admin@codementor.ai",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsEmailVerified = true,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Create user profile
            var profile = new UserProfile
            {
                UserId = adminUser.Id,
                Level = 1,
                XpPoints = 0,
                BugsFixed = 0,
                GamesWon = 0,
                CurrentStreak = 0,
                PreferredLanguages = System.Text.Json.JsonSerializer.Serialize(new[] { "JavaScript" }),
                LearningGoals = "System Administration",
                LastActiveDate = DateTime.UtcNow
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Admin user created successfully",
                email = "admin@codementor.ai",
                password = "admin123"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user");
            return StatusCode(500, new { message = "Failed to create admin user" });
        }
    }

    private bool IsAdmin()
    {
        var isAdminClaim = User.FindFirst("IsAdmin");
        return isAdminClaim != null && bool.TryParse(isAdminClaim.Value, out bool isAdmin) && isAdmin;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    private AdminUserDto MapToAdminUserDto(User user)
    {
        return new AdminUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.ProfilePictureUrl,
            IsAdmin = user.IsAdmin,
            IsEmailVerified = user.IsEmailVerified,
            Level = user.Profile?.Level ?? 1,
            XpPoints = user.Profile?.XpPoints ?? 0,
            BugsFixed = user.Profile?.BugsFixed ?? 0,
            GamesWon = user.Profile?.GamesWon ?? 0,
            CurrentStreak = user.Profile?.CurrentStreak ?? 0,
            LastLoginAt = user.LastLoginAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = user.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private AdminSessionDto MapToAdminSessionDto(CollaborationSession session)
    {
        return new AdminSessionDto
        {
            Id = session.Id,
            RoomId = session.RoomId,
            Name = session.Name,
            Description = session.Description,
            Language = session.Language,
            IsPublic = session.IsPublic,
            IsActive = session.IsActive,
            Status = session.Status,
            OwnerName = session.Owner.Name,
            OwnerEmail = session.Owner.Email,
            ParticipantCount = session.Participants.Count(p => p.IsActive),
            CreatedAt = session.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = session.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private AdminRoadmapDto MapToAdminRoadmapDto(Roadmap roadmap)
    {
        var topics = new List<string>();
        var goals = new List<string>();
        
        try
        {
            if (!string.IsNullOrEmpty(roadmap.Topics))
                topics = System.Text.Json.JsonSerializer.Deserialize<List<string>>(roadmap.Topics) ?? new List<string>();
            if (!string.IsNullOrEmpty(roadmap.Goals))
                goals = System.Text.Json.JsonSerializer.Deserialize<List<string>>(roadmap.Goals) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deserializing roadmap topics/goals for roadmap {RoadmapId}", roadmap.Id);
        }

        return new AdminRoadmapDto
        {
            Id = roadmap.Id,
            Title = roadmap.Title,
            Description = roadmap.Description,
            Category = roadmap.Category,
            Difficulty = roadmap.Difficulty,
            EstimatedDuration = roadmap.EstimatedDuration,
            Topics = topics,
            Goals = goals,
            AuthorName = roadmap.Author?.Name ?? "Unknown",
            AuthorEmail = roadmap.Author?.Email ?? "Unknown",
            Status = roadmap.Status,
            Enrollments = roadmap.Enrollments?.Count ?? 0,
            CreatedAt = roadmap.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = roadmap.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool? IsAdmin { get; set; }
    public int? Level { get; set; }
    public int? XpPoints { get; set; }
    public int? BugsFixed { get; set; }
    public int? GamesWon { get; set; }
    public int? CurrentStreak { get; set; }
}

public class AdminUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsEmailVerified { get; set; }
    public int Level { get; set; }
    public int XpPoints { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
    public int CurrentStreak { get; set; }
    public string? LastLoginAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class AdminSessionDto
{
    public int Id { get; set; }
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalGameResults { get; set; }
    public int TotalCodeSnippets { get; set; }
    public int TotalRoadmaps { get; set; }
    public int TotalEditors { get; set; }
    public List<AdminActivityDto> RecentActivity { get; set; } = new();
}

public class AdminActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class AdminRoadmapDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public List<string> Goals { get; set; } = new();
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Enrollments { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class CreateRoadmapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public List<string> Goals { get; set; } = new();
}

public class UpdateRoadmapRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Difficulty { get; set; }
    public string? EstimatedDuration { get; set; }
    public string? Status { get; set; }
    public List<string>? Topics { get; set; }
    public List<string>? Goals { get; set; }
}
