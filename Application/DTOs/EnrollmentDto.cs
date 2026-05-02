using System.ComponentModel.DataAnnotations;
using SmartLMS.Domain.Enums;

namespace SmartLMS.Application.DTOs;

public record EnrollStudentRequest
{
    [Required]
    public required int CourseId { get; init; }
    
    [Required]
    public required int UserId { get; init; }
}

public record EnrollmentResponse
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public int CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public EnrollmentStatus Status { get; init; }
    public DateTime EnrolledAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
