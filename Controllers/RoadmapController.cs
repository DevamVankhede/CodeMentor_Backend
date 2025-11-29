using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CodeMentorAI.API.Data;
using CodeMentorAI.API.Models;
using System.Security.Claims;
using static CodeMentorAI.API.Controllers.AdminController;

namespace CodeMentorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoadmapController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoadmapController> _logger;

    public RoadmapController(AppDbContext context, ILogger<RoadmapController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoadmapDto>>> GetRoadmaps([FromQuery] string? category = null, [FromQuery] string? difficulty = null)
    {
        try
        {
            var query = _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .Where(r => r.Status == "active");

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(r => r.Difficulty.ToLower() == difficulty.ToLower());
            }

            var roadmaps = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var roadmapDtos = roadmaps.Select(MapToRoadmapDto).ToList();
            return Ok(roadmapDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roadmaps");
            return StatusCode(500, new { message = "Failed to fetch roadmaps" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoadmapDto>> GetRoadmap(int id)
    {
        try
        {
            var roadmap = await _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == "active");

            if (roadmap == null)
            {
                return NotFound(new { message = "Roadmap not found" });
            }

            return Ok(MapToRoadmapDto(roadmap));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roadmap");
            return StatusCode(500, new { message = "Failed to fetch roadmap" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RoadmapDto>> CreateRoadmap([FromBody] CreateRoadmapRequest request)
    {
        try
        {
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

            return Ok(MapToRoadmapDto(roadmapWithAuthor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating roadmap");
            return StatusCode(500, new { message = "Failed to create roadmap" });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<RoadmapDto>> UpdateRoadmap(int id, [FromBody] UpdateRoadmapRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var roadmap = await _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (roadmap == null)
            {
                return NotFound(new { message = "Roadmap not found" });
            }

            // Check if user owns the roadmap or is admin
            var isAdmin = IsAdmin();
            if (roadmap.AuthorId != userId && !isAdmin)
            {
                return Forbid("You can only edit your own roadmaps");
            }

            // Update roadmap properties
            roadmap.Title = request.Title ?? roadmap.Title;
            roadmap.Description = request.Description ?? roadmap.Description;
            roadmap.Category = request.Category ?? roadmap.Category;
            roadmap.Difficulty = request.Difficulty ?? roadmap.Difficulty;
            roadmap.EstimatedDuration = request.EstimatedDuration ?? roadmap.EstimatedDuration;
            
            if (request.Topics != null)
                roadmap.Topics = System.Text.Json.JsonSerializer.Serialize(request.Topics);
            if (request.Goals != null)
                roadmap.Goals = System.Text.Json.JsonSerializer.Serialize(request.Goals);
            
            roadmap.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToRoadmapDto(roadmap));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roadmap");
            return StatusCode(500, new { message = "Failed to update roadmap" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteRoadmap(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var roadmap = await _context.Roadmaps.FindAsync(id);
            if (roadmap == null)
            {
                return NotFound(new { message = "Roadmap not found" });
            }

            // Check if user owns the roadmap or is admin
            var isAdmin = IsAdmin();
            if (roadmap.AuthorId != userId && !isAdmin)
            {
                return Forbid("You can only delete your own roadmaps");
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

    [HttpPost("{id}/enroll")]
    [Authorize]
    public async Task<ActionResult> EnrollInRoadmap(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var roadmap = await _context.Roadmaps.FindAsync(id);
            if (roadmap == null)
            {
                return NotFound(new { message = "Roadmap not found" });
            }

            // Check if already enrolled
            var existingEnrollment = await _context.RoadmapEnrollments
                .FirstOrDefaultAsync(e => e.RoadmapId == id && e.UserId == userId);

            if (existingEnrollment != null)
            {
                return BadRequest(new { message = "Already enrolled in this roadmap" });
            }

            var enrollment = new RoadmapEnrollment
            {
                RoadmapId = id,
                UserId = userId.Value,
                Progress = 0,
                EnrolledAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            _context.RoadmapEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully enrolled in roadmap" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling in roadmap");
            return StatusCode(500, new { message = "Failed to enroll in roadmap" });
        }
    }

    [HttpGet("my-roadmaps")]
    [Authorize]
    public async Task<ActionResult<List<RoadmapDto>>> GetMyRoadmaps()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var roadmaps = await _context.Roadmaps
                .Include(r => r.Author)
                .Include(r => r.Enrollments)
                .Where(r => r.AuthorId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var roadmapDtos = roadmaps.Select(MapToRoadmapDto).ToList();
            return Ok(roadmapDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user roadmaps");
            return StatusCode(500, new { message = "Failed to fetch your roadmaps" });
        }
    }

    [HttpGet("enrolled")]
    [Authorize]
    public async Task<ActionResult<List<EnrolledRoadmapDto>>> GetEnrolledRoadmaps()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var enrollments = await _context.RoadmapEnrollments
                .Include(e => e.Roadmap)
                    .ThenInclude(r => r.Author)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.LastAccessedAt)
                .ToListAsync();

            var enrolledRoadmaps = enrollments.Select(e => new EnrolledRoadmapDto
            {
                Id = e.Roadmap.Id,
                Title = e.Roadmap.Title,
                Description = e.Roadmap.Description,
                Category = e.Roadmap.Category,
                Difficulty = e.Roadmap.Difficulty,
                EstimatedDuration = e.Roadmap.EstimatedDuration,
                AuthorName = e.Roadmap.Author?.Name ?? "Unknown",
                Progress = e.Progress,
                EnrolledAt = e.EnrolledAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                LastAccessedAt = e.LastAccessedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }).ToList();

            return Ok(enrolledRoadmaps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching enrolled roadmaps");
            return StatusCode(500, new { message = "Failed to fetch enrolled roadmaps" });
        }
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

    private bool IsAdmin()
    {
        var isAdminClaim = User.FindFirst("IsAdmin");
        return isAdminClaim != null && bool.TryParse(isAdminClaim.Value, out bool isAdmin) && isAdmin;
    }

    private RoadmapDto MapToRoadmapDto(Roadmap roadmap)
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

        return new RoadmapDto
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
            Enrollments = roadmap.Enrollments?.Count ?? 0,
            CreatedAt = roadmap.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}

public class RoadmapDto
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
    public int Enrollments { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class EnrolledRoadmapDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string EnrolledAt { get; set; } = string.Empty;
    public string LastAccessedAt { get; set; } = string.Empty;
}

