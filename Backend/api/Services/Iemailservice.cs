using InterviewScheduler.API.Models;

namespace InterviewScheduler.API.Services
{
    public interface IEmailService
    {
        Task SendInterviewNotificationAsync(Interview interview);
        Task SendInterviewUpdateNotificationAsync(Interview interview);
        Task SendInterviewCancellationNotificationAsync(Interview interview);
    }
}