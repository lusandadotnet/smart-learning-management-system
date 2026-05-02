<<<<<<< HEAD
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.DTOs;
using SmartLMS.Infrastructure.Persistance;
using System.Security.Claims;

namespace SmartLMS.Infrastructure.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            return NotFound(new { message = "User not found" });

        var response = new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Get all users (Administrator only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                ProfilePictureUrl = u.ProfilePictureUrl,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");
        
        return int.Parse(userIdClaim);
    }
}
=======
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLMS.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
    }
}
>>>>>>> origin/main
