using System.ComponentModel.DataAnnotations;

namespace InterviewScheduler.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string GoogleId { get; set; }
        
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string? ProfilePicture { get; set; }
        
        [StringLength(500)]
        public string? AccessToken { get; set; }
        
        [StringLength(500)]
        public string? RefreshToken { get; set; }
        
        // Removed TokenExpires property
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    }
}