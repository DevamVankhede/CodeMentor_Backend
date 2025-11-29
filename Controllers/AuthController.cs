using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.Models;
using CodeMentorAI.API.DTOs;

namespace CodeMentorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> Signup([FromBody] SignupRequest request)
    {
        try
        {
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
                IsEmailVerified = true, // Auto-verify for demo
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

            var token = GenerateJwtToken(userWithProfile);

            return Ok(new AuthResponse
            {
                User = MapToUserDto(userWithProfile),
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup");
            return StatusCode(500, new { message = "Registration failed" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            if (user.Profile != null)
            {
                user.Profile.LastActiveDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                User = MapToUserDto(user),
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "Login failed" });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "Failed to get user data" });
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Update user info
            if (!string.IsNullOrEmpty(request.Name))
            {
                user.Name = request.Name;
            }
            user.UpdatedAt = DateTime.UtcNow;

            // Update profile
            if (user.Profile != null)
            {
                if (request.PreferredLanguages != null)
                {
                    user.Profile.PreferredLanguages = System.Text.Json.JsonSerializer.Serialize(request.PreferredLanguages);
                }
                if (!string.IsNullOrEmpty(request.LearningGoals))
                {
                    user.Profile.LearningGoals = request.LearningGoals;
                }
                user.Profile.LastActiveDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { message = "Profile update failed" });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "your-super-secret-key-that-is-at-least-32-characters-long"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("IsAdmin", user.IsAdmin.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "CodeMentorAI",
            audience: _configuration["Jwt:Audience"] ?? "CodeMentorAI",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Avatar = user.ProfilePictureUrl,
            Level = user.Profile?.Level ?? 1,
            Xp = user.Profile?.XpPoints ?? 0,
            BugsFixed = user.Profile?.BugsFixed ?? 0,
            GamesWon = user.Profile?.GamesWon ?? 0,
            Streak = user.Profile?.CurrentStreak ?? 0,
            CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsAdmin = user.IsAdmin
        };
    }
}