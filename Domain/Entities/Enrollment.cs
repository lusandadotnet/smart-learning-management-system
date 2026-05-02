using System;
using SmartLMS.Domain.Enums;

namespace SmartLMS.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }

    // Foreign Keys
    public int CourseId { get; set; }
    public int UserId { get; set; } // Renamed for consistency with your User entity

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    // The status of the student's progress in the course
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    // Progress tracking (e.g., 0 to 100)
    public decimal ProgressPercentage { get; set; }

    // nav props
    
    // The student who is enrolled
    public virtual User User { get; set; } = null!;

    // The course they are taking
    public virtual Course Course { get; set; } = null!;
}

