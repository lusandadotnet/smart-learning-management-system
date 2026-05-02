using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Azure;
using Azure.AI.OpenAI;
using SmartLMS.Application.Interfaces;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Enums;
using DomainChatRole = SmartLMS.Domain.Enums.ChatRole; 
using SmartLMS.Infrastructure.Persistance; 

namespace SmartLMS.Application.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly OpenAIClient _openAiClient;

    public ChatService(AppDbContext context, OpenAIClient openAiClient)
    {
        _context = context;
        _openAiClient = openAiClient;
    }

    public async Task<string> SendMessageAsync(int userId, int courseId, int? lessonId, string userMessage)
    {
        // 1. SECURITY: Verify the user is actively enrolled in this course
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
            
        if (!isEnrolled)
            throw new UnauthorizedAccessException("You do not have active access to this course's tutor.");

        // 2. FETCH BRAIN: Get the AI Configuration for this specific course
        var aiConfig = await _context.AiTutorConfigurations
            .FirstOrDefaultAsync(c => c.CourseId == courseId) 
            ?? throw new Exception("AI Tutor is not configured for this course.");

        // 3. MEMORY: Get or Create the Chat Session
        var session = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CourseId == courseId && s.LessonId == lessonId);

        if (session == null)
        {
            session = new ChatSession { UserId = userId, CourseId = courseId, LessonId = lessonId };
            _context.ChatSessions.Add(session);
        }

        // Update activity tracking
        session.LastActiveAt = DateTime.UtcNow;

        // 4. SAVE USER MESSAGE to database
        var userDbMessage = new ChatMessage 
        { 
            Role = DomainChatRole.User, 
            Content = userMessage 
        };
        session.Messages.Add(userDbMessage);

        // 5. BUILD AZURE AI REQUEST
        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            DeploymentName = aiConfig.DeploymentName, // e.g., "gpt-4o"
            Temperature = aiConfig.Temperature,
            MaxTokens = aiConfig.MaxTokens
        };

        // Add System Prompt
        var systemPrompt = aiConfig.SystemPrompt;

        // 6. GROUNDING (Context Injection)
        if (lessonId.HasValue)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId.Value);
            if (lesson != null && !string.IsNullOrWhiteSpace(lesson.ContentBody))
            {
                systemPrompt += $"\n\nContext for current lesson ({lesson.Title}):\n{lesson.ContentBody}";
            }
        }
        
        chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(systemPrompt));

        // Add Conversation History (Limit to last 10 messages to save tokens)
        var recentHistory = session.Messages.Where(m => m.Role != DomainChatRole.System).TakeLast(10);
        foreach (var msg in recentHistory)
        {
            if (msg.Role == DomainChatRole.User)
                chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(msg.Content));
            else if (msg.Role == DomainChatRole.Assistant)
                chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(msg.Content));
        }

        // 7. CALL AZURE OPENAI
        Response<ChatCompletions> response = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
        string aiResponseText = response.Value.Choices[0].Message.Content;

        // 8. SAVE AI RESPONSE to database
        var aiDbMessage = new ChatMessage 
        { 
            Role = DomainChatRole.Assistant, 
            Content = aiResponseText,
            TokenCount = response.Value.Usage.TotalTokens // Track your API costs!
        };
        session.Messages.Add(aiDbMessage);

        await _context.SaveChangesAsync();

        return aiResponseText;
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(int userId)
    {
        // Fetches all chat sessions for the user, ordered by most recent activity
        return await _context.ChatSessions
            .Include(s => s.Course)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastActiveAt ?? s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetSessionHistoryAsync(int sessionId, int userId)
    {
        // Fetches the specific back-and-forth dialogue for a session
        var session = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
            throw new UnauthorizedAccessException("Session not found or access denied.");

        return session.Messages.OrderBy(m => m.Timestamp).ToList();
    }



    // ... GetUserSessionsAsync and GetSessionHistoryAsync implementation omitted for brevity ...
}