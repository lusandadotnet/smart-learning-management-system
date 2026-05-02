using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.DTOs;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Enums;
using SmartLMS.Infrastructure.Persistance;
using System.Security.Claims;

namespace SmartLMS.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrollmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Enroll a student in a course (Instructor or Administrator only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<ActionResult<EnrollmentResponse>> EnrollStudent([FromBody] EnrollStudentRequest request)
    {
        var currentUserId = GetCurrentUserId();

        // Verify course exists
        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null)
            return NotFound(new { message = "Course not found" });

        // Authorization: Only the course instructor or admin can enroll students
        if (course.InstructorId != currentUserId && !User.IsInRole("Administrator"))
            return Forbid();

        // Verify student exists
        var student = await _context.Users.FindAsync(request.UserId);
        if (student == null)
            return NotFound(new { message = "Student not found" });

        // Check if already enrolled
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.CourseId == request.CourseId);

        if (existingEnrollment != null)
            return BadRequest(new { message = "Student is already enrolled in this course" });

        var enrollment = new Enrollment
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        var response = new EnrollmentResponse
        {
            Id = enrollment.Id,
            UserId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}",
            StudentEmail = student.Email,
            CourseId = course.Id,
            CourseTitle = course.Title,
            Status = enrollment.Status,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt
        };

        return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.Id }, response);
    }

    /// <summary>
    /// Get enrollment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentResponse>> GetEnrollment(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound(new { message = "Enrollment not found" });

        var currentUserId = GetCurrentUserId();

        // Authorization: Student can view their own enrollment, instructor can view their course enrollments
        if (enrollment.UserId != currentUserId && 
            enrollment.Course.InstructorId != currentUserId && 
            !User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        var response = new EnrollmentResponse
        {
            Id = enrollment.Id,
            UserId = enrollment.User.Id,
            StudentName = $"{enrollment.User.FirstName} {enrollment.User.LastName}",
            StudentEmail = enrollment.User.Email,
            CourseId = enrollment.Course.Id,
            CourseTitle = enrollment.Course.Title,
            Status = enrollment.Status,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Get my enrollments (Student view)
    /// </summary>
    [HttpGet("my-enrollments")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<EnrollmentResponse>>> GetMyEnrollments()
    {
        var userId = GetCurrentUserId();

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Instructor)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                StudentName = $"{e.User.FirstName} {e.User.LastName}",
                StudentEmail = e.User.Email,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                Status = e.Status,
                EnrolledAt = e.EnrolledAt,
                CompletedAt = e.CompletedAt
            })
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>
    /// Update enrollment status (Instructor or Administrator only)
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> UpdateEnrollmentStatus(int id, [FromBody] EnrollmentStatus status)
    {
        var currentUserId = GetCurrentUserId();

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound(new { message = "Enrollment not found" });

        // Authorization: Only course instructor or admin can update status
        if (enrollment.Course.InstructorId != currentUserId && !User.IsInRole("Administrator"))
            return Forbid();

        enrollment.Status = status;

        if (status == EnrollmentStatus.Completed && !enrollment.CompletedAt.HasValue)
            enrollment.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Unenroll a student (Instructor or Administrator only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var currentUserId = GetCurrentUserId();

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound(new { message = "Enrollment not found" });

        // Authorization: Only course instructor or admin can delete
        if (enrollment.Course.InstructorId != currentUserId && !User.IsInRole("Administrator"))
            return Forbid();

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");
        
        return int.Parse(userIdClaim);
    }
}
