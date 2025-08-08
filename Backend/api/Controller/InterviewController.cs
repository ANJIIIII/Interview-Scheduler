using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InterviewScheduler.API.Data;
using InterviewScheduler.API.Models;
using InterviewScheduler.API.DTOs;
using InterviewScheduler.API.Services;

namespace InterviewScheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InterviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoogleCalendarService _calendarService;
        private readonly IEmailService _emailService;

        public InterviewController(
            ApplicationDbContext context,
            IGoogleCalendarService calendarService,
            IEmailService emailService)
        {
            _context = context;
            _calendarService = calendarService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterviewResponseDto>>> GetInterviews()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var interviews = await _context.Interviews
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InterviewResponseDto
                {
                    Id = i.Id,
                    JobTitle = i.JobTitle,
                    CandidateName = i.CandidateName,
                    CandidateEmail = i.CandidateEmail,
                    InterviewerName = i.InterviewerName,
                    InterviewerEmail = i.InterviewerEmail,
                    StartTime = i.StartTime,
                    EndTime = i.EndTime,
                    GoogleMeetLink = i.GoogleMeetLink,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            return Ok(interviews);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterviewResponseDto>> GetInterview(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var interview = await _context.Interviews
                .Where(i => i.Id == id && i.UserId == userId)
                .Select(i => new InterviewResponseDto
                {
                    Id = i.Id,
                    JobTitle = i.JobTitle,
                    CandidateName = i.CandidateName,
                    CandidateEmail = i.CandidateEmail,
                    InterviewerName = i.InterviewerName,
                    InterviewerEmail = i.InterviewerEmail,
                    StartTime = i.StartTime,
                    EndTime = i.EndTime,
                    GoogleMeetLink = i.GoogleMeetLink,
                    CreatedAt = i.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPost]
        public async Task<ActionResult<InterviewResponseDto>> CreateInterview([FromBody] CreateInterviewDto request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            // Validate the request
            if (request.StartTime >= request.EndTime)
                return BadRequest("End time must be after start time");

            if (request.StartTime <= DateTime.UtcNow)
                return BadRequest("Interview time must be in the future");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            try
            {
                // Create interview record
                var interview = new Interview
                {
                    JobTitle = request.JobTitle,
                    CandidateName = request.CandidateName,
                    CandidateEmail = request.CandidateEmail,
                    InterviewerName = request.InterviewerName,
                    InterviewerEmail = request.InterviewerEmail,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    UserId = userId.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Interviews.Add(interview);
                await _context.SaveChangesAsync();

                // Create Google Calendar event if user has access token
                if (!string.IsNullOrEmpty(user.AccessToken))
                {
                    try
                    {
                        var meetLink = await _calendarService.CreateCalendarEventAsync(interview, user.AccessToken);
                        interview.GoogleMeetLink = meetLink;
                        
                        _context.Interviews.Update(interview);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception calendarEx)
                    {
                        // Log calendar error but don't fail the interview creation
                        Console.WriteLine($"Calendar creation failed: {calendarEx.Message}");
                    }
                }

                // Send email notifications
                try
                {
                    await _emailService.SendInterviewNotificationAsync(interview);
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the interview creation
                    Console.WriteLine($"Email notification failed: {emailEx.Message}");
                }

                var response = new InterviewResponseDto
                {
                    Id = interview.Id,
                    JobTitle = interview.JobTitle,
                    CandidateName = interview.CandidateName,
                    CandidateEmail = interview.CandidateEmail,
                    InterviewerName = interview.InterviewerName,
                    InterviewerEmail = interview.InterviewerEmail,
                    StartTime = interview.StartTime,
                    EndTime = interview.EndTime,
                    GoogleMeetLink = interview.GoogleMeetLink,
                    CreatedAt = interview.CreatedAt
                };

                return CreatedAtAction(nameof(GetInterview), new { id = interview.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create interview: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<InterviewResponseDto>> UpdateInterview(int id, [FromBody] CreateInterviewDto request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var interview = await _context.Interviews
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (interview == null)
                return NotFound();

            // Validate the request
            if (request.StartTime >= request.EndTime)
                return BadRequest("End time must be after start time");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            try
            {
                // Update interview
                interview.JobTitle = request.JobTitle;
                interview.CandidateName = request.CandidateName;
                interview.CandidateEmail = request.CandidateEmail;
                interview.InterviewerName = request.InterviewerName;
                interview.InterviewerEmail = request.InterviewerEmail;
                interview.StartTime = request.StartTime;
                interview.EndTime = request.EndTime;
                interview.UpdatedAt = DateTime.UtcNow;

                _context.Interviews.Update(interview);
                await _context.SaveChangesAsync();

                // Update Google Calendar event if user has access token
                if (!string.IsNullOrEmpty(user.AccessToken))
                {
                    try
                    {
                        await _calendarService.UpdateCalendarEventAsync(interview, user.AccessToken);
                    }
                    catch (Exception calendarEx)
                    {
                        Console.WriteLine($"Calendar update failed: {calendarEx.Message}");
                    }
                }

                // Send updated email notifications
                try
                {
                    await _emailService.SendInterviewUpdateNotificationAsync(interview);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Email notification failed: {emailEx.Message}");
                }

                var response = new InterviewResponseDto
                {
                    Id = interview.Id,
                    JobTitle = interview.JobTitle,
                    CandidateName = interview.CandidateName,
                    CandidateEmail = interview.CandidateEmail,
                    InterviewerName = interview.InterviewerName,
                    InterviewerEmail = interview.InterviewerEmail,
                    StartTime = interview.StartTime,
                    EndTime = interview.EndTime,
                    GoogleMeetLink = interview.GoogleMeetLink,
                    CreatedAt = interview.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update interview: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInterview(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var interview = await _context.Interviews
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (interview == null)
                return NotFound();

            try
            {
                // Delete from Google Calendar if event exists
                if (!string.IsNullOrEmpty(interview.GoogleEventId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.AccessToken))
                    {
                        try
                        {
                            await _calendarService.DeleteCalendarEventAsync(interview.GoogleEventId, user.AccessToken);
                        }
                        catch (Exception calendarEx)
                        {
                            Console.WriteLine($"Calendar deletion failed: {calendarEx.Message}");
                        }
                    }
                }

                // Send cancellation email notifications
                try
                {
                    await _emailService.SendInterviewCancellationNotificationAsync(interview);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Email notification failed: {emailEx.Message}");
                }

                _context.Interviews.Remove(interview);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to delete interview: {ex.Message}");
            }
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }
    }
}