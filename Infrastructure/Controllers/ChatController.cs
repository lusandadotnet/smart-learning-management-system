using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLMS.Application.DTOs;
using SmartLMS.Application.Interfaces;
using SmartLMS.Domain.Enums;
using System.Security.Claims;

namespace SmartLMS.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// Send a message to the AI tutor for a specific course
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<string>> SendMessage([FromBody] SendChatMessageRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var response = await _chatService.SendMessageAsync(
                userId, 
                request.CourseId, 
                request.LessonId, 
                request.Message
            );

            return Ok(new { response });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all chat sessions for the current user
    /// </summary>
    [HttpGet("sessions")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<ChatSessionResponse>>> GetMySessions()
    {
        var userId = GetCurrentUserId();

        var sessions = await _chatService.GetUserSessionsAsync(userId);

        var response = sessions.Select(s => new ChatSessionResponse
        {
            Id = s.Id,
            CourseId = s.CourseId,
            CourseTitle = s.Course?.Title ?? "Unknown",
            LessonId = s.LessonId,
            LessonTitle = s.Lesson?.Title,
            CreatedAt = s.CreatedAt,
            LastActiveAt = s.LastActiveAt,
            MessageCount = s.Messages.Count
        });

        return Ok(response);
    }

    /// <summary>
    /// Get the full chat history for a specific session
    /// </summary>
    [HttpGet("sessions/{sessionId}/history")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<ChatMessageResponse>>> GetSessionHistory(int sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();

            var messages = await _chatService.GetSessionHistoryAsync(sessionId, userId);

            var response = messages.Select(m => new ChatMessageResponse
            {
                Role = m.Role.ToString(),
                Content = m.Content,
                Timestamp = m.Timestamp
            });

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");
        
        return int.Parse(userIdClaim);
    }
}
