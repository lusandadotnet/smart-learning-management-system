using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Xunit;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Enums;
using SmartLMS.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace SmartLMS.Tests.Repositories;

public class CourseRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CourseRepository _repository;
    private readonly string _connectionString;

    public CourseRepositoryTests()
    {
        _connectionString = "Server=(localdb)\\mssqllocaldb;Database=SmartLMS_Test_" + Guid.NewGuid() + ";Trusted_Connection=True;";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", _connectionString)
            })
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new CourseRepository(configuration);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var instructor = new User
        {
            EntraObjectId = "test-123",
            FirstName = "Test",
            LastName = "Instructor",
            Email = "instructor@test.com",
            Role = UserRole.Instructor
        };

        var student = new User
        {
            EntraObjectId = "student-456",
            FirstName = "Test",
            LastName = "Student",
            Email = "student@test.com",
            Role = UserRole.Student
        };

        _context.Users.AddRange(instructor, student);
        _context.SaveChanges();

        var course = new Course
        {
            Title = "Advanced C# Programming",
            Description = "Master C# development",
            InstructorId = instructor.Id,
            IsPublished = true
        };

        _context.Courses.Add(course);
        _context.SaveChanges();

        var module = new Module
        {
            CourseId = course.Id,
            Title = "LINQ Fundamentals",
            SequenceOrder = 1
        };

        _context.Modules.Add(module);

        var enrollment = new Enrollment
        {
            UserId = student.Id,
            CourseId = course.Id,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllCoursesAsync_ReturnsCoursesWithAggregates()
    {
        // Act
        var courses = await _repository.GetAllCoursesAsync();

        // Assert
        var courseList = courses.ToList();
        Assert.Single(courseList);
        
        var course = courseList.First();
        Assert.Equal("Advanced C# Programming", course.Title);
        Assert.Equal("Test Instructor", course.InstructorName);
        Assert.Equal(1, course.ModuleCount);
        Assert.Equal(1, course.EnrollmentCount);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithValidId_ReturnsCourse()
    {
        // Arrange
        var existingCourse = _context.Courses.First();

        // Act
        var course = await _repository.GetCourseByIdAsync(existingCourse.Id);

        // Assert
        Assert.NotNull(course);
        Assert.Equal("Advanced C# Programming", course.Title);
        Assert.Equal(1, course.ModuleCount);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var course = await _repository.GetCourseByIdAsync(9999);

        // Assert
        Assert.Null(course);
    }

    [Fact]
    public async Task GetCourseEnrollmentsAsync_ReturnsEnrollments()
    {
        // Arrange
        var courseId = _context.Courses.First().Id;

        // Act
        var enrollments = await _repository.GetCourseEnrollmentsAsync(courseId);

        // Assert
        var enrollmentList = enrollments.ToList();
        Assert.Single(enrollmentList);
        
        var enrollment = enrollmentList.First();
        Assert.Equal("Test Student", enrollment.StudentName);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
    }

    [Fact]
    public async Task GetInstructorCoursesAsync_ReturnsInstructorCourses()
    {
        // Arrange
        var instructorId = _context.Users.First(u => u.Role == UserRole.Instructor).Id;

        // Act
        var courses = await _repository.GetInstructorCoursesAsync(instructorId);

        // Assert
        var courseList = courses.ToList();
        Assert.Single(courseList);
        Assert.Equal("Advanced C# Programming", courseList.First().Title);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
