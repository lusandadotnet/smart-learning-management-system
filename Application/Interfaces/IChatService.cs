using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLMS.Domain.Entities;

namespace SmartLMS.Application.Interfaces;

public interface IChatService
{
    // the core ai assistant method
    Task<string> SendMessageAsync(int userId, int courseId, int? lessonId, string userMessage);
    
    // retrieve chat sessions and history
    Task<List<ChatSession>> GetUserSessionsAsync(int userId);
    Task<List<ChatMessage>> GetSessionHistoryAsync(int sessionId, int userId);
}