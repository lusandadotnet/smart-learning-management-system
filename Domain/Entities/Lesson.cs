using System;

namespace SmartLMS.Domain.Entities;

public class Lesson
{
    public int Id {get; set;}

    public int ModuleId { get; set; } // fk to module    
    public required string Title { get; set; } 
    public string? ContentBody { get; set; }
    public int SequenceOrder { get; set; }

    // navigation props
    // parent
    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<LessonMaterial> Materials { get; set; } = new HashSet<LessonMaterial>();

    // The AI Context: Sessions can be tied to a specific lesson
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new HashSet<ChatSession>();
}
