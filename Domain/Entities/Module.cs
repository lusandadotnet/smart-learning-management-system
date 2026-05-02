using System;

namespace SmartLMS.Domain.Entities;

public class Module
{
    public int Id { get; set; }

    //foreign key
    public int CourseId { get; set; }
    public required string Title { get; set; } 

    // order of this module within the course
    public required int SequenceOrder { get; set; }


    // navigation props
    public virtual Course Course { get; set; } = null!; // parent course
    public virtual ICollection<Lesson> Lessons { get; set; } = new HashSet<Lesson>(); // lessons within this module
}
