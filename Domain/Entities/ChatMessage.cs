using System;
using SmartLMS.Domain.Enums;

namespace SmartLMS.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    
    // Foreign Key
    public int ChatSessionId { get; set; }

    // Role: System, User, or Assistant
    public ChatRole Role { get; set; }

    // in OnModelCreating, this must be NVARCHAR(MAX).
    public required string Content { get; set; }

    // track costs and usage per message
    public int TokenCount { get; set; } 

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // -navigation properties-
    
    // Allows you to navigate from a message back to the session context
    public virtual ChatSession ChatSession { get; set; } = null!;
}