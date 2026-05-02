using System;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities;

public class ChatSession
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public int CourseId { get; set; }
    
    public int? LessonId { get; set; }

    
    public string? Summary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }

    // nav props

    // The student owning this conversation
    public virtual User User { get; set; } = null!;

    // The course context
    public virtual Course Course { get; set; } = null!;

    // The specific lesson context 
    public virtual Lesson? Lesson { get; set; }

    // The actual back-and-forth dialogue
    public virtual ICollection<ChatMessage> Messages { get; set; } = new HashSet<ChatMessage>();
}