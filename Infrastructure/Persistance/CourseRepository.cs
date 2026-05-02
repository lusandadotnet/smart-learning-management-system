using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SmartLMS.Application.DTOs;
using SmartLMS.Application.Interfaces;

namespace SmartLMS.Infrastructure.Persistance;

public class CourseRepository : ICourseRepository
{
    private readonly string _connectionString;

    public CourseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection not found");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT 
                c.Id,
                c.Title,
                c.Description,
                c.IsPublished,
                c.InstructorId,
                c.CreatedAt,
                CONCAT(u.FirstName, ' ', u.LastName) AS InstructorName,
                COUNT(DISTINCT m.Id) AS ModuleCount,
                COUNT(DISTINCT e.Id) AS EnrollmentCount
            FROM Courses c
            INNER JOIN Users u ON c.InstructorId = u.Id
            LEFT JOIN Modules m ON c.Id = m.CourseId
            LEFT JOIN Enrollments e ON c.Id = e.CourseId AND e.Status = 0
            WHERE u.IsDeleted = 0
            GROUP BY c.Id, c.Title, c.Description, c.IsPublished, 
                     c.InstructorId, c.CreatedAt, u.FirstName, u.LastName
            ORDER BY c.CreatedAt DESC";

        return await connection.QueryAsync<CourseResponse>(sql);
    }

    public async Task<CourseResponse?> GetCourseByIdAsync(int courseId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT 
                c.Id,
                c.Title,
                c.Description,
                c.IsPublished,
                c.InstructorId,
                c.CreatedAt,
                CONCAT(u.FirstName, ' ', u.LastName) AS InstructorName,
                COUNT(DISTINCT m.Id) AS ModuleCount,
                COUNT(DISTINCT e.Id) AS EnrollmentCount
            FROM Courses c
            INNER JOIN Users u ON c.InstructorId = u.Id
            LEFT JOIN Modules m ON c.Id = m.CourseId
            LEFT JOIN Enrollments e ON c.Id = e.CourseId AND e.Status = 0
            WHERE c.Id = @CourseId AND u.IsDeleted = 0
            GROUP BY c.Id, c.Title, c.Description, c.IsPublished, 
                     c.InstructorId, c.CreatedAt, u.FirstName, u.LastName";

        return await connection.QueryFirstOrDefaultAsync<CourseResponse>(sql, new { CourseId = courseId });
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetCourseEnrollmentsAsync(int courseId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT 
                e.Id,
                e.UserId,
                CONCAT(u.FirstName, ' ', u.LastName) AS StudentName,
                u.Email AS StudentEmail,
                e.CourseId,
                c.Title AS CourseTitle,
                e.Status,
                e.EnrolledAt,
                e.CompletedAt
            FROM Enrollments e
            INNER JOIN Users u ON e.UserId = u.Id
            INNER JOIN Courses c ON e.CourseId = c.Id
            WHERE e.CourseId = @CourseId AND u.IsDeleted = 0
            ORDER BY e.EnrolledAt DESC";

        return await connection.QueryAsync<EnrollmentResponse>(sql, new { CourseId = courseId });
    }

    public async Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT 
                c.Id,
                c.Title,
                c.Description,
                c.IsPublished,
                c.InstructorId,
                c.CreatedAt,
                CONCAT(u.FirstName, ' ', u.LastName) AS InstructorName,
                COUNT(DISTINCT m.Id) AS ModuleCount,
                COUNT(DISTINCT e.Id) AS EnrollmentCount
            FROM Courses c
            INNER JOIN Users u ON c.InstructorId = u.Id
            LEFT JOIN Modules m ON c.Id = m.CourseId
            LEFT JOIN Enrollments e ON c.Id = e.CourseId AND e.Status = 0
            WHERE c.InstructorId = @InstructorId AND u.IsDeleted = 0
            GROUP BY c.Id, c.Title, c.Description, c.IsPublished, 
                     c.InstructorId, c.CreatedAt, u.FirstName, u.LastName
            ORDER BY c.CreatedAt DESC";

        return await connection.QueryAsync<CourseResponse>(sql, new { InstructorId = instructorId });
    }
}
