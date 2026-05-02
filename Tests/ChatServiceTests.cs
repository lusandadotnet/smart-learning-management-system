using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Azure;
using Azure.AI.OpenAI;
using SmartLMS.Application.Services;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Enums;
using SmartLMS.Infrastructure.Persistance;

namespace SmartLMS.Tests.Services;

public class ChatServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<OpenAIClient> _mockOpenAIClient;
    private readonly ChatService _chatService;

    public ChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockOpenAIClient = new Mock<OpenAIClient>();
        _chatService = new ChatService(_context, _mockOpenAIClient.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var instructor = new User
        {
            Id = 1,
            EntraObjectId = "instructor-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Role = UserRole.Instructor
        };

        var student = new User
        {
            Id = 2,
            EntraObjectId = "student-456",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Role = UserRole.Student
        };

        var course = new Course
        {
            Id = 1,
            Title = "Introduction to C#",
            Description = "Learn C# fundamentals",
            InstructorId = 1,
            IsPublished = true
        };

        var module = new Module
        {
            Id = 1,
            CourseId = 1,
            Title = "Variables and Data Types",
            SequenceOrder = 1
        };

        var lesson = new Lesson
        {
            Id = 1,
            ModuleId = 1,
            Title = "Introduction to Variables",
            ContentBody = "Variables store data values. In C#, you declare variables with a type.",
            SequenceOrder = 1
        };

        var enrollment = new Enrollment
        {
            Id = 1,
            UserId = 2,
            CourseId = 1,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };

        var aiConfig = new AiTutorConfiguration
        {
            Id = 1,
            CourseId = 1,
            DeploymentName = "gpt-4o",
            SystemPrompt = "You are a helpful programming tutor.",
            Temperature = 0.7f,
            MaxTokens = 500
        };

        _context.Users.AddRange(instructor, student);
        _context.Courses.Add(course);
        _context.Modules.Add(module);
        _context.Lessons.Add(lesson);
        _context.Enrollments.Add(enrollment);
        _context.AiTutorConfigurations.Add(aiConfig);
        _context.SaveChanges();
    }

    [Fact]
    public async Task SendMessageAsync_WithValidEnrollment_ReturnsAIResponse()
    {
        // Arrange
        int userId = 2; // Student
        int courseId = 1;
        int lessonId = 1;
        string userMessage = "What is a variable?";
        string aiResponse = "A variable is a container for storing data values.";

        var mockResponse = CreateMockChatCompletionsResponse(aiResponse, 50);
        _mockOpenAIClient
            .Setup(x => x.GetChatCompletionsAsync(It.IsAny<ChatCompletionsOptions>(), default))
            .ReturnsAsync(Response.FromValue(mockResponse, Mock.Of<Response>()));

        // Act
        var result = await _chatService.SendMessageAsync(userId, courseId, lessonId, userMessage);

        // Assert
        Assert.Equal(aiResponse, result);
        
        var session = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync();
        
        Assert.NotNull(session);
        Assert.Equal(2, session.Messages.Count); // User + Assistant
        Assert.Equal(userMessage, session.Messages.First().Content);
        Assert.Equal(aiResponse, session.Messages.Last().Content);
    }

    [Fact]
    public async Task SendMessageAsync_WithoutEnrollment_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        int userId = 1; // Instructor (not enrolled as student)
        int courseId = 1;
        string userMessage = "Test message";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _chatService.SendMessageAsync(userId, courseId, null, userMessage)
        );
    }

    [Fact]
    public async Task SendMessageAsync_WithoutAIConfig_ThrowsException()
    {
        // Arrange
        var newCourse = new Course
        {
            Id = 2,
            Title = "Test Course",
            InstructorId = 1,
            IsPublished = true
        };
        _context.Courses.Add(newCourse);

        var newEnrollment = new Enrollment
        {
            UserId = 2,
            CourseId = 2,
            Status = EnrollmentStatus.Active
        };
        _context.Enrollments.Add(newEnrollment);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _chatService.SendMessageAsync(2, 2, null, "Test")
        );
    }

    [Fact]
    public async Task GetUserSessionsAsync_ReturnsOrderedSessions()
    {
        // Arrange
        int userId = 2;

        // Create multiple sessions
        var session1 = new ChatSession
        {
            UserId = userId,
            CourseId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            LastActiveAt = DateTime.UtcNow.AddDays(-2)
        };

        var session2 = new ChatSession
        {
            UserId = userId,
            CourseId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastActiveAt = DateTime.UtcNow
        };

        _context.ChatSessions.AddRange(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _chatService.GetUserSessionsAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].LastActiveAt >= result[1].LastActiveAt);
    }

    [Fact]
    public async Task GetSessionHistoryAsync_WithValidSession_ReturnsMessages()
    {
        // Arrange
        int userId = 2;
        var session = new ChatSession
        {
            UserId = userId,
            CourseId = 1
        };

        session.Messages.Add(new ChatMessage
        {
            Role = ChatRole.User,
            Content = "Hello",
            Timestamp = DateTime.UtcNow.AddMinutes(-5)
        });

        session.Messages.Add(new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = "Hi there!",
            Timestamp = DateTime.UtcNow
        });

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _chatService.GetSessionHistoryAsync(session.Id, userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello", result[0].Content);
        Assert.Equal("Hi there!", result[1].Content);
    }

    [Fact]
    public async Task GetSessionHistoryAsync_WithWrongUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = 2,
            CourseId = 1
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _chatService.GetSessionHistoryAsync(session.Id, userId: 1) // Wrong user
        );
    }

    private ChatCompletions CreateMockChatCompletionsResponse(string content, int totalTokens)
    {
        // Create a mock response using reflection since ChatCompletions is not easily mockable
        var message = ChatResponseMessage.CreateAssistantMessage(content);
        var choice = new ChatChoice(0, message, ChatFinishReason.Stopped, null);
        var usage = new CompletionsUsage(10, 40, totalTokens);
        
        return OpenAIModelFactory.ChatCompletions(
            id: "test-id",
            created: DateTimeOffset.UtcNow,
            choices: new[] { choice },
            usage: usage
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
