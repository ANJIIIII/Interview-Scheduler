// DTOs/AuthDto.cs
namespace InterviewScheduler.API.DTOs
{
    public class GoogleAuthDto
    {
        public string Code { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
    }
}