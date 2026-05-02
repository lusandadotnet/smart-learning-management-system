using System;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    
    // Foreign Key to the User (Instructor)
    public int InstructorId { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // nav props

    public virtual User Instructor { get; set; } = null!;

    // course is made of many modules
    public virtual ICollection<Module> Modules { get; set; } = new HashSet<Module>();

    // track who is allowed to access this course
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();

    // Configuration for the Azure AI Tutor
    public virtual AiTutorConfiguration? AiTutorConfiguration { get; set; }
}