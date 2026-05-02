using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


using SmartLMS.Domain.Entities;

namespace SmartLMS.Infrastructure.Persistance;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<AiTutorConfiguration> AiTutorConfigurations { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<LessonMaterial> LessonMaterials { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 1. User Identity Constraints
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasIndex(u => u.EntraObjectId).IsUnique();
        entity.HasIndex(u => u.Email).IsUnique();
        
        entity.Property(u => u.FirstName).HasMaxLength(100);
        entity.Property(u => u.LastName).HasMaxLength(100);
        entity.Property(u => u.Email).HasMaxLength(256);
        entity.Property(u => u.ProfilePictureUrl).HasMaxLength(1000);
        
        // Store the Role enum as a readable string ("Student", "Instructor")
        entity.Property(u => u.Role).HasConversion<string>();
    });

    // 2. Enrollment: The Security Bridge
    modelBuilder.Entity<Enrollment>(entity =>
    {
        // Prevents a student from being enrolled in the same course twice
        entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        
        entity.Property(e => e.Status).HasConversion<string>();
        entity.Property(e => e.ProgressPercentage).HasPrecision(5, 2); // e.g., 100.00
    });

    // 3. The 1-to-1 AI Tutor Configuration
    modelBuilder.Entity<Course>(entity =>
    {
        entity.HasOne(c => c.AiTutorConfiguration)
              .WithOne(a => a.Course)
              .HasForeignKey<AiTutorConfiguration>(a => a.CourseId)
              .OnDelete(DeleteBehavior.Cascade); // If course is deleted, AI config goes too
    });

    // 4. AI Memory: Chat Session & Messages
    modelBuilder.Entity<ChatSession>(entity =>
    {
        entity.HasMany(s => s.Messages)
              .WithOne(m => m.ChatSession)
              .HasForeignKey(m => m.ChatSessionId);
    });

    modelBuilder.Entity<ChatMessage>(entity =>
    {
        // AI responses can be thousands of words; NVARCHAR(MAX) is required
        entity.Property(m => m.Content).HasColumnType("nvarchar(max)");
        entity.Property(m => m.Role).HasConversion<string>();
    });

    // 5. Curriculum Content
    modelBuilder.Entity<Lesson>(entity =>
    {
        entity.Property(l => l.Title).HasMaxLength(200);
        entity.Property(l => l.ContentBody).HasColumnType("nvarchar(max)");
    });

    modelBuilder.Entity<LessonMaterial>(entity =>
    {
        entity.Property(lm => lm.BlobUri).HasMaxLength(1000);
        entity.Property(lm => lm.StorageFileName).HasMaxLength(500);
    });

    // 6. Global Delete Behavior Policy
    // Prevents accidental data loss by disabling default cascade deletes
    foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
    {
        relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }
}


}
