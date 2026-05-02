using System;
using SmartLMS.Domain.Enums;

namespace SmartLMS.Domain.Entities;

public class User
{
    public int Id { get; set; } // primary key
    public required string EntraObjectId { get; set; } // entra id uid
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public required string Email { get; set; } 

    //auth and profile management 
    public UserRole Role { get; set; } 
    public string? ProfilePictureUrl { get; set; } // blob url

    //audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public bool IsDeleted { get; set; } = false;

    

    // navigation props
    // courses this user is enrolled in 
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();

    // ai chat history for this user
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new HashSet<ChatSession>();

    // courses this user manages (if they are an Instructor)
    public virtual ICollection<Course> ManagedCourses { get; set; } = new HashSet<Course>();
}
