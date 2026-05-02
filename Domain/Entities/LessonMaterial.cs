using System;

namespace SmartLMS.Domain.Entities;

public class LessonMaterial
{
    public int Id { get; set; }

    // foreign key
    public int LessonId { get; set; }
    public required string FileName { get; set; } 

    // meta data about the file
    public required string ContentType { get; set; }
    public required long SizeInBytes { get; set; }

    public required string  BlobUri { get; set; } // URI to the file in blob storage

    // guid used in storage to avoid filename collisions
    public required string StorageFileName { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // navigation prop
    public virtual Lesson Lesson { get; set; } = null!; // parent lesson
}
