using SmartLMS.Domain.Enums;

namespace SmartLMS.Application.DTOs;

public record UserResponse
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
