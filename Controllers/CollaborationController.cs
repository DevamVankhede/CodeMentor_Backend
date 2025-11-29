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
public class CollaborationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CollaborationController> _logger;

    public CollaborationController(AppDbContext context, ILogger<CollaborationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<CollaborationSessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            // Generate unique room ID
            var roomId = Guid.NewGuid().ToString("N")[..12];

            var session = new CollaborationSession
            {
                RoomId = roomId,
                Name = request.Name,
                Description = request.Description,
                OwnerId = userId.Value,
                Code = request.InitialCode ?? "",
                Language = request.Language,
                IsPublic = request.IsPublic,
                IsActive = true,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CollaborationSessions.Add(session);
            await _context.SaveChangesAsync();

            // Add owner as participant
            var participant = new CollaborationParticipant
            {
                SessionId = session.Id,
                UserId = userId.Value,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.CollaborationParticipants.Add(participant);
            await _context.SaveChangesAsync();

            // Load session with owner and participants
            var createdSession = await _context.CollaborationSessions
                .Include(s => s.Owner)
                .Include(s => s.Participants)
                    .ThenInclude(p => p.User)
                .FirstAsync(s => s.Id == session.Id);

            return Ok(MapToSessionDto(createdSession));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating collaboration session");
            return StatusCode(500, new { message = "Failed to create session" });
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<CollaborationSessionDto>>> GetSessions()
    {
        try
        {
            var sessions = await _context.CollaborationSessions
                .Include(s => s.Owner)
                .Include(s => s.Participants.Where(p => p.IsActive))
                    .ThenInclude(p => p.User)
                .Where(s => s.IsActive && s.IsPublic)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var sessionDtos = sessions.Select(MapToSessionDto).ToList();
            return Ok(sessionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching collaboration sessions");
            return StatusCode(500, new { message = "Failed to fetch sessions" });
        }
    }

    [HttpGet("sessions/{roomId}")]
    public async Task<ActionResult<CollaborationSessionDto>> GetSession(string roomId)
    {
        try
        {
            var session = await _context.CollaborationSessions
                .Include(s => s.Owner)
                .Include(s => s.Participants.Where(p => p.IsActive))
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.RoomId == roomId && s.IsActive);

            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            return Ok(MapToSessionDto(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching collaboration session");
            return StatusCode(500, new { message = "Failed to fetch session" });
        }
    }

    [HttpPost("sessions/{roomId}/join")]
    public async Task<ActionResult> JoinSession(string roomId)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _context.CollaborationSessions
                .FirstOrDefaultAsync(s => s.RoomId == roomId && s.IsActive);

            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            // Check if user is already a participant
            var existingParticipant = await _context.CollaborationParticipants
                .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.UserId == userId.Value);

            if (existingParticipant != null)
            {
                if (!existingParticipant.IsActive)
                {
                    existingParticipant.IsActive = true;
                    existingParticipant.LeftAt = null;
                    await _context.SaveChangesAsync();
                }
                return Ok(new { message = "Joined session successfully" });
            }

            // Add new participant
            var participant = new CollaborationParticipant
            {
                SessionId = session.Id,
                UserId = userId.Value,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.CollaborationParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Joined session successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining collaboration session");
            return StatusCode(500, new { message = "Failed to join session" });
        }
    }

    [HttpPost("sessions/{roomId}/leave")]
    public async Task<ActionResult> LeaveSession(string roomId)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _context.CollaborationSessions
                .FirstOrDefaultAsync(s => s.RoomId == roomId && s.IsActive);

            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            var participant = await _context.CollaborationParticipants
                .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.UserId == userId.Value && p.IsActive);

            if (participant != null)
            {
                participant.IsActive = false;
                participant.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Left session successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving collaboration session");
            return StatusCode(500, new { message = "Failed to leave session" });
        }
    }

    [HttpPut("sessions/{roomId}/code")]
    public async Task<ActionResult> UpdateCode(string roomId, [FromBody] UpdateCodeRequest request)
    {
        try
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var session = await _context.CollaborationSessions
                .FirstOrDefaultAsync(s => s.RoomId == roomId && s.IsActive);

            if (session == null)
            {
                return NotFound(new { message = "Session not found" });
            }

            // Check if user is a participant
            var isParticipant = await _context.CollaborationParticipants
                .AnyAsync(p => p.SessionId == session.Id && p.UserId == userId.Value && p.IsActive);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this session");
            }

            session.Code = request.Code;
            session.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Code updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session code");
            return StatusCode(500, new { message = "Failed to update code" });
        }
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

    private CollaborationSessionDto MapToSessionDto(CollaborationSession session)
    {
        return new CollaborationSessionDto
        {
            Id = session.Id,
            RoomId = session.RoomId,
            Name = session.Name,
            Description = session.Description,
            Language = session.Language,
            Code = session.Code,
            IsPublic = session.IsPublic,
            IsActive = session.IsActive,
            Status = session.Status,
            CreatedAt = session.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = session.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Owner = new UserDto
            {
                Id = session.Owner.Id.ToString(),
                Name = session.Owner.Name,
                Email = session.Owner.Email,
                Avatar = session.Owner.ProfilePictureUrl
            },
            Participants = session.Participants.Where(p => p.IsActive).Select(p => new UserDto
            {
                Id = p.User.Id.ToString(),
                Name = p.User.Name,
                Email = p.User.Email,
                Avatar = p.User.ProfilePictureUrl
            }).ToList(),
            ParticipantCount = session.Participants.Count(p => p.IsActive)
        };
    }
}

public class CreateSessionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Language { get; set; } = "javascript";
    public string? InitialCode { get; set; }
    public bool IsPublic { get; set; } = true;
}

public class UpdateCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public class CollaborationSessionDto
{
    public int Id { get; set; }
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public UserDto Owner { get; set; } = null!;
    public List<UserDto> Participants { get; set; } = new();
    public int ParticipantCount { get; set; }
}
