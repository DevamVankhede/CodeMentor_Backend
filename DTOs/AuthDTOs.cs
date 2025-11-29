using System.ComponentModel.DataAnnotations;

namespace CodeMentorAI.API.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class SignupRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string? Name { get; set; }
    public string[]? PreferredLanguages { get; set; }
    public string? LearningGoals { get; set; }
}

public class AuthResponse
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public int BugsFixed { get; set; }
    public int GamesWon { get; set; }
    public int Streak { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}