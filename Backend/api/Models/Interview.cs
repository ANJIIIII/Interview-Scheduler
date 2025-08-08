using System.ComponentModel.DataAnnotations;

namespace InterviewScheduler.API.Models
{
    public class Interview
    {
        public int Id { get; set; }
        
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
        
        public string? GoogleMeetLink { get; set; }
        
        public string? GoogleEventId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
