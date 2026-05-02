using System.ComponentModel.DataAnnotations;

namespace SmartLMS.Application.DTOs;

public record SendChatMessageRequest
{
    [Required]
    public required int CourseId { get; init; }
    
    public int? LessonId { get; init; }
    
    [Required, MinLength(1), MaxLength(4000)]
    public required string Message { get; init; }
}

public record ChatMessageResponse
{
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record ChatSessionResponse
{
    public int Id { get; init; }
    public int CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public int? LessonId { get; init; }
    public string? LessonTitle { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActiveAt { get; init; }
    public int MessageCount { get; init; }
}
