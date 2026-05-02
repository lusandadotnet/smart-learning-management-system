using SmartLMS.Application.DTOs;

namespace SmartLMS.Application.Interfaces;

/// <summary>
/// Dapper repository for high-performance read operations
/// Used for analytics dashboards and reporting queries
/// </summary>
public interface ICourseRepository
{
    Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
    Task<CourseResponse?> GetCourseByIdAsync(int courseId);
    Task<IEnumerable<EnrollmentResponse>> GetCourseEnrollmentsAsync(int courseId);
    Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId);
}
