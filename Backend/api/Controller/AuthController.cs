// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using InterviewScheduler.API.Data;
using InterviewScheduler.API.Models;
using InterviewScheduler.API.DTOs;

namespace InterviewScheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("google")]
        public async Task<ActionResult<AuthResponseDto>> GoogleAuth([FromBody] GoogleAuthDto request)
        {
            Console.WriteLine("=== GoogleAuth endpoint hit ===");
            Console.WriteLine($"Request is null: {request == null}");
            
            if (request != null)
            {
                Console.WriteLine($"Code is null or empty: {string.IsNullOrEmpty(request.Code)}");
                Console.WriteLine($"Code length: {request.Code?.Length ?? 0}");
                Console.WriteLine($"Code (first 20 chars): {request.Code?.Substring(0, Math.Min(20, request.Code?.Length ?? 0))}...");
            }
            
            try
            {
                // Add validation
                if (request == null)
                {
                    Console.WriteLine("ERROR: Request object is null");
                    return BadRequest("Request body is required");
                }
                
                if (string.IsNullOrEmpty(request.Code))
                {
                    Console.WriteLine("ERROR: Authorization code is missing");
                    return BadRequest("Authorization code is required");
                }

                Console.WriteLine("Validation passed, proceeding with token exchange...");

                // Exchange authorization code for access token
                var httpClient = new HttpClient();
                var tokenRequest = new Dictionary<string, string>
                {
                    {"code", request.Code},
                    {"client_id", _configuration["GoogleAuth:ClientId"]},
                    {"client_secret", _configuration["GoogleAuth:ClientSecret"]},
                    {"redirect_uri", _configuration["GoogleAuth:RedirectUri"]},
                    {"grant_type", "authorization_code"}
                };

                Console.WriteLine("hehe1 - About to call Google token endpoint");
                Console.WriteLine($"Using redirect_uri: {_configuration["GoogleAuth:RedirectUri"]}");

                var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(tokenRequest));

                Console.WriteLine($"Google token response status: {tokenResponse.StatusCode}");

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Google token error: {errorContent}");
                    return BadRequest($"Failed to exchange authorization code: {errorContent}");
                }

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(tokenContent);

                var accessToken = tokenData["access_token"].ToString();
                var refreshToken = tokenData.ContainsKey("refresh_token") ? tokenData["refresh_token"].ToString() : null;

                // Get user info from Google
                var userInfoResponse = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    var userInfoError = await userInfoResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Google userinfo error: {userInfoError}");
                    return BadRequest($"Failed to get user information: {userInfoError}");
                }

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfo = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userInfoContent);

                var googleId = userInfo["id"].ToString();
                var email = userInfo["email"].ToString();
                var name = userInfo["name"].ToString();
                var picture = userInfo.ContainsKey("picture") ? userInfo["picture"].ToString() : null;
                
                Console.WriteLine("hehe2 - User info retrieved successfully");
                Console.WriteLine($"User email: {email}");
                
                // Find or create user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
                
                if (user == null)
                {
                    user = new User
                    {
                        GoogleId = googleId,
                        Email = email,
                        Name = name,
                        ProfilePicture = picture,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _context.Users.Add(user);
                    Console.WriteLine("Creating new user");
                }
                else
                {
                    user.AccessToken = accessToken;
                    if (refreshToken != null)
                        user.RefreshToken = refreshToken;
                    user.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Users.Update(user);
                    Console.WriteLine("Updating existing user");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("hehe3 - Database operations completed");

                // Generate JWT token
                var jwtToken = GenerateJwtToken(user);

                var response = new AuthResponseDto
                {
                    Token = jwtToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        ProfilePicture = user.ProfilePicture
                    }
                };
                
                Console.WriteLine("hehe5 - Response created successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GoogleAuth: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Authentication failed: {ex.Message}");
            }
        }
         

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                ProfilePicture = user.ProfilePicture
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:ExpiryInDays"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }
    }
}