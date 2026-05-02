using System.ComponentModel.DataAnnotations;

namespace SmartLMS.Application.DTOs;

public record CreateCourseRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }
    
    [MaxLength(2000)]
    public string? Description { get; init; }
}

public record UpdateCourseRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }
    
    [MaxLength(2000)]
    public string? Description { get; init; }
    
    public bool? IsPublished { get; init; }
}

public record CourseResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPublished { get; init; }
    public int InstructorId { get; init; }
    public string InstructorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int ModuleCount { get; init; }
    public int EnrollmentCount { get; init; }
}
