using InterviewScheduler.API.Models;

namespace InterviewScheduler.API.Services
{
    public interface IGoogleCalendarService
    {
        Task<string> CreateCalendarEventAsync(Interview interview, string accessToken);
        Task<bool> DeleteCalendarEventAsync(string eventId, string accessToken);
        Task<bool> UpdateCalendarEventAsync(Interview interview, string accessToken);
    }
}