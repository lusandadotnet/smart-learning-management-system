using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.DTOs;
using SmartLMS.Application.Interfaces;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Enums;
using SmartLMS.Infrastructure.Persistance;
using System.Security.Claims;

namespace SmartLMS.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICourseRepository _courseRepository;

    public CoursesController(AppDbContext context, ICourseRepository courseRepository)
    {
        _context = context;
        _courseRepository = courseRepository;
    }

    /// <summary>
    /// Get all courses (uses Dapper for performance)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAllCourses()
    {
        var courses = await _courseRepository.GetAllCoursesAsync();
        return Ok(courses);
    }

    /// <summary>
    /// Get course by ID (uses Dapper for performance)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CourseResponse>> GetCourse(int id)
    {
        var course = await _courseRepository.GetCourseByIdAsync(id);
        
        if (course == null)
            return NotFound(new { message = "Course not found" });

        return Ok(course);
    }

    /// <summary>
    /// Create a new course (Instructor only, uses EF Core)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<ActionResult<CourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = GetCurrentUserId();
        
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            InstructorId = userId,
            IsPublished = false
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var createdCourse = await _courseRepository.GetCourseByIdAsync(course.Id);
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, createdCourse);
    }

    /// <summary>
    /// Update course (Instructor only, uses EF Core)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        var userId = GetCurrentUserId();
        
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound(new { message = "Course not found" });

        // Authorization: Only the instructor who created the course can update it
        if (course.InstructorId != userId && !User.IsInRole("Administrator"))
            return Forbid();

        if (request.Title != null)
            course.Title = request.Title;
        
        if (request.Description != null)
            course.Description = request.Description;
        
        if (request.IsPublished.HasValue)
            course.IsPublished = request.IsPublished.Value;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete course (Instructor only, uses EF Core)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var userId = GetCurrentUserId();
        
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound(new { message = "Course not found" });

        // Authorization: Only the instructor who created the course can delete it
        if (course.InstructorId != userId && !User.IsInRole("Administrator"))
            return Forbid();

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get my courses (Instructor only, uses Dapper)
    /// </summary>
    [HttpGet("my-courses")]
    [Authorize(Roles = "Instructor")]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetMyCourses()
    {
        var userId = GetCurrentUserId();
        var courses = await _courseRepository.GetInstructorCoursesAsync(userId);
        return Ok(courses);
    }

    /// <summary>
    /// Get enrollments for a course (Instructor only, uses Dapper)
    /// </summary>
    [HttpGet("{id}/enrollments")]
    [Authorize(Roles = "Instructor,Administrator")]
    public async Task<ActionResult<IEnumerable<EnrollmentResponse>>> GetCourseEnrollments(int id)
    {
        var userId = GetCurrentUserId();
        
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound(new { message = "Course not found" });

        // Authorization: Only the instructor of this course can view enrollments
        if (course.InstructorId != userId && !User.IsInRole("Administrator"))
            return Forbid();

        var enrollments = await _courseRepository.GetCourseEnrollmentsAsync(id);
        return Ok(enrollments);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");
        
        return int.Parse(userIdClaim);
    }
}
