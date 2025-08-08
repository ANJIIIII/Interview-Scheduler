using System.ComponentModel.DataAnnotations;

namespace InterviewScheduler.API.DTOs
{
    public class CreateInterviewDto
    {
        [Required]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string CandidateName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CandidateEmail { get; set; } = string.Empty;

        [Required]
        public string InterviewerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string InterviewerEmail { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }

    public class InterviewResponseDto
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string InterviewerName { get; set; } = string.Empty;
        public string InterviewerEmail { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? GoogleMeetLink { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}