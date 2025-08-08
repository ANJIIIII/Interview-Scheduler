using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using InterviewScheduler.API.Models;
using Microsoft.EntityFrameworkCore;
using InterviewScheduler.API.Data;

namespace InterviewScheduler.API.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public GoogleCalendarService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<string> CreateCalendarEventAsync(Interview interview, string accessToken)
        {
            try
            {
                var credential = GoogleCredential.FromAccessToken(accessToken);
                
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Interview Scheduler"
                });

                var calendarEvent = new Event()
                {
                    Summary = $"Interview: {interview.JobTitle}",
                    Description = $@"
Interview Details:
- Position: {interview.JobTitle}
- Candidate: {interview.CandidateName} ({interview.CandidateEmail})
- Interviewer: {interview.InterviewerName} ({interview.InterviewerEmail})

This is an automated interview scheduled through Interview Scheduler.
                    ".Trim(),
                    Start = new EventDateTime()
                    {
                        DateTime = interview.StartTime,
                        TimeZone = "UTC"
                    },
                    End = new EventDateTime()
                    {
                        DateTime = interview.EndTime,
                        TimeZone = "UTC"
                    },
                    Attendees = new List<EventAttendee>()
                    {
                        new EventAttendee() 
                        { 
                            Email = interview.CandidateEmail,
                            DisplayName = interview.CandidateName
                        },
                        new EventAttendee() 
                        { 
                            Email = interview.InterviewerEmail,
                            DisplayName = interview.InterviewerName
                        }
                    },
                    ConferenceData = new ConferenceData()
                    {
                        CreateRequest = new CreateConferenceRequest()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            ConferenceSolutionKey = new ConferenceSolutionKey()
                            {
                                Type = "hangoutsMeet"
                            }
                        }
                    },
                    Reminders = new Event.RemindersData()
                    {
                        UseDefault = false,
                        Overrides = new List<EventReminder>()
                        {
                            new EventReminder()
                            {
                                Method = "email",
                                Minutes = 1440 // 24 hours
                            },
                            new EventReminder()
                            {
                                Method = "popup",
                                Minutes = 15
                            }
                        }
                    }
                };

                var request = service.Events.Insert(calendarEvent, "primary");
                request.ConferenceDataVersion = 1;
                request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;
                
                var createdEvent = await request.ExecuteAsync();
                
                // Store the Google Event ID for future reference
                interview.GoogleEventId = createdEvent.Id;
                await _context.SaveChangesAsync();
                
                return createdEvent.HangoutLink ?? createdEvent.HtmlLink ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the interview creation
                Console.WriteLine($"Google Calendar API Error: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> DeleteCalendarEventAsync(string eventId, string accessToken)
        {
            try
            {
                var credential = GoogleCredential.FromAccessToken(accessToken);
                
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Interview Scheduler"
                });

                var request = service.Events.Delete("primary", eventId);
                request.SendUpdates = EventsResource.DeleteRequest.SendUpdatesEnum.All;
                
                await request.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Calendar Delete Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCalendarEventAsync(Interview interview, string accessToken)
        {
            try
            {
                if (string.IsNullOrEmpty(interview.GoogleEventId))
                {
                    // If no existing event, create a new one
                    var meetLink = await CreateCalendarEventAsync(interview, accessToken);
                    interview.GoogleMeetLink = meetLink;
                    return true;
                }

                var credential = GoogleCredential.FromAccessToken(accessToken);
                
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Interview Scheduler"
                });

                // Get the existing event
                var existingEvent = await service.Events.Get("primary", interview.GoogleEventId).ExecuteAsync();

                // Update the event details
                existingEvent.Summary = $"Interview: {interview.JobTitle}";
                existingEvent.Description = $@"
Interview Details:
- Position: {interview.JobTitle}
- Candidate: {interview.CandidateName} ({interview.CandidateEmail})
- Interviewer: {interview.InterviewerName} ({interview.InterviewerEmail})

This is an automated interview scheduled through Interview Scheduler.
                ".Trim();
                
                existingEvent.Start = new EventDateTime()
                {
                    DateTime = interview.StartTime,
                    TimeZone = "UTC"
                };
                
                existingEvent.End = new EventDateTime()
                {
                    DateTime = interview.EndTime,
                    TimeZone = "UTC"
                };

                existingEvent.Attendees = new List<EventAttendee>()
                {
                    new EventAttendee() 
                    { 
                        Email = interview.CandidateEmail,
                        DisplayName = interview.CandidateName
                    },
                    new EventAttendee() 
                    { 
                        Email = interview.InterviewerEmail,
                        DisplayName = interview.InterviewerName
                    }
                };

                var updateRequest = service.Events.Update(existingEvent, "primary", interview.GoogleEventId);
                updateRequest.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
                
                await updateRequest.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Calendar Update Error: {ex.Message}");
                return false;
            }
        }
    }
}