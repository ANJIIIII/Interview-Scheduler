using MailKit.Net.Smtp;
using MimeKit;
using InterviewScheduler.API.Models;

namespace InterviewScheduler.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendInterviewNotificationAsync(Interview interview)
        {
            var candidateMessage = CreateEmailMessage(
                interview.CandidateEmail,
                interview.CandidateName,
                "Interview Scheduled - Action Required",
                CreateCandidateEmailBody(interview)
            );

            var interviewerMessage = CreateEmailMessage(
                interview.InterviewerEmail,
                interview.InterviewerName,
                "New Interview Scheduled",
                CreateInterviewerEmailBody(interview)
            );

            await SendEmailAsync(candidateMessage);
            await SendEmailAsync(interviewerMessage);
        }

        public async Task SendInterviewUpdateNotificationAsync(Interview interview)
        {
            var candidateMessage = CreateEmailMessage(
                interview.CandidateEmail,
                interview.CandidateName,
                "Interview Updated - Please Review",
                CreateCandidateUpdateEmailBody(interview)
            );

            var interviewerMessage = CreateEmailMessage(
                interview.InterviewerEmail,
                interview.InterviewerName,
                "Interview Updated",
                CreateInterviewerUpdateEmailBody(interview)
            );

            await SendEmailAsync(candidateMessage);
            await SendEmailAsync(interviewerMessage);
        }

        public async Task SendInterviewCancellationNotificationAsync(Interview interview)
        {
            var candidateMessage = CreateEmailMessage(
                interview.CandidateEmail,
                interview.CandidateName,
                "Interview Cancelled",
                CreateCandidateCancellationEmailBody(interview)
            );

            var interviewerMessage = CreateEmailMessage(
                interview.InterviewerEmail,
                interview.InterviewerName,
                "Interview Cancelled",
                CreateInterviewerCancellationEmailBody(interview)
            );

            await SendEmailAsync(candidateMessage);
            await SendEmailAsync(interviewerMessage);
        }

        private MimeMessage CreateEmailMessage(string toEmail, string toName, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["EmailSettings:FromName"],
                _configuration["EmailSettings:FromEmail"]
            ));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            return message;
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    true
                );
                
                await client.AuthenticateAsync(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                );
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }

        private string CreateCandidateEmailBody(Interview interview)
        {
            var meetLinkSection = !string.IsNullOrEmpty(interview.GoogleMeetLink) 
                ? $"<li><strong>Google Meet Link:</strong> <a href='{interview.GoogleMeetLink}' style='color: #2563eb; text-decoration: none;'>Join Meeting</a></li>"
                : "<li><strong>Meeting Link:</strong> Will be provided separately</li>";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #2563eb; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        ul {{ padding-left: 0; list-style: none; }}
                        li {{ margin: 10px 0; padding: 8px; background: #f3f4f6; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 12px; }}
                        .button {{ background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Interview Scheduled ✅</h2>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{interview.CandidateName}</strong>,</p>
                            <p>Congratulations! Your interview has been scheduled for the <strong>{interview.JobTitle}</strong> position.</p>
                            
                            <div class='details'>
                                <h3>📋 Interview Details:</h3>
                                <ul>
                                    <li><strong>Position:</strong> {interview.JobTitle}</li>
                                    <li><strong>Interviewer:</strong> {interview.InterviewerName}</li>
                                    <li><strong>Date & Time:</strong> {interview.StartTime:dddd, MMMM dd, yyyy} at {interview.StartTime:HH:mm} UTC</li>
                                    <li><strong>Duration:</strong> {(interview.EndTime - interview.StartTime).TotalMinutes} minutes</li>
                                    {meetLinkSection}
                                </ul>
                            </div>
                            
                            <p><strong>⚠️ Important Reminders:</strong></p>
                            <ul>
                                <li>Please join the meeting 5 minutes early</li>
                                <li>Test your camera and microphone beforehand</li>
                                <li>Have a copy of your resume ready</li>
                                <li>Prepare questions about the role and company</li>
                            </ul>
                            
                            <p>We look forward to speaking with you. Good luck! 🍀</p>
                            
                            <div class='footer'>
                                <p>Best regards,<br><strong>Interview Scheduler Team</strong></p>
                                <p>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string CreateInterviewerEmailBody(Interview interview)
        {
            var meetLinkSection = !string.IsNullOrEmpty(interview.GoogleMeetLink) 
                ? $"<li><strong>Google Meet Link:</strong> <a href='{interview.GoogleMeetLink}' style='color: #2563eb; text-decoration: none;'>Join Meeting</a></li>"
                : "<li><strong>Meeting Link:</strong> Will be provided separately</li>";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #059669; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        ul {{ padding-left: 0; list-style: none; }}
                        li {{ margin: 10px 0; padding: 8px; background: #f3f4f6; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>New Interview Scheduled 📅</h2>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{interview.InterviewerName}</strong>,</p>
                            <p>A new interview has been scheduled with a candidate for the <strong>{interview.JobTitle}</strong> position.</p>
                            
                            <div class='details'>
                                <h3>📋 Interview Details:</h3>
                                <ul>
                                    <li><strong>Position:</strong> {interview.JobTitle}</li>
                                    <li><strong>Candidate:</strong> {interview.CandidateName}</li>
                                    <li><strong>Candidate Email:</strong> <a href='mailto:{interview.CandidateEmail}'>{interview.CandidateEmail}</a></li>
                                    <li><strong>Date & Time:</strong> {interview.StartTime:dddd, MMMM dd, yyyy} at {interview.StartTime:HH:mm} UTC</li>
                                    <li><strong>Duration:</strong> {(interview.EndTime - interview.StartTime).TotalMinutes} minutes</li>
                                    {meetLinkSection}
                                </ul>
                            </div>
                            
                            <p><strong>📝 Preparation Notes:</strong></p>
                            <ul>
                                <li>Please join the meeting 5 minutes early</li>
                                <li>Review the candidate's profile beforehand</li>
                                <li>Prepare relevant interview questions</li>
                                <li>Have the job description ready for reference</li>
                            </ul>
                            
                            <p>The interview has been added to your calendar with all necessary details.</p>
                            
                            <div class='footer'>
                                <p>Best regards,<br><strong>Interview Scheduler Team</strong></p>
                                <p>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string CreateCandidateUpdateEmailBody(Interview interview)
        {
            var meetLinkSection = !string.IsNullOrEmpty(interview.GoogleMeetLink) 
                ? $"<li><strong>Google Meet Link:</strong> <a href='{interview.GoogleMeetLink}' style='color: #2563eb; text-decoration: none;'>Join Meeting</a></li>"
                : "<li><strong>Meeting Link:</strong> Will be provided separately</li>";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #d97706; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        ul {{ padding-left: 0; list-style: none; }}
                        li {{ margin: 10px 0; padding: 8px; background: #f3f4f6; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 12px; }}
                        .update-notice {{ background: #fef3c7; padding: 15px; border-left: 4px solid #d97706; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Interview Updated 🔄</h2>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{interview.CandidateName}</strong>,</p>
                            
                            <div class='update-notice'>
                                <strong>⚠️ Important:</strong> Your interview details have been updated. Please review the new information below.
                            </div>
                            
                            <div class='details'>
                                <h3>📋 Updated Interview Details:</h3>
                                <ul>
                                    <li><strong>Position:</strong> {interview.JobTitle}</li>
                                    <li><strong>Interviewer:</strong> {interview.InterviewerName}</li>
                                    <li><strong>Date & Time:</strong> {interview.StartTime:dddd, MMMM dd, yyyy} at {interview.StartTime:HH:mm} UTC</li>
                                    <li><strong>Duration:</strong> {(interview.EndTime - interview.StartTime).TotalMinutes} minutes</li>
                                    {meetLinkSection}
                                </ul>
                            </div>
                            
                            <p>Please make note of these changes and update your calendar accordingly.</p>
                            
                            <div class='footer'>
                                <p>Best regards,<br><strong>Interview Scheduler Team</strong></p>
                                <p>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string CreateInterviewerUpdateEmailBody(Interview interview)
        {
            var meetLinkSection = !string.IsNullOrEmpty(interview.GoogleMeetLink) 
                ? $"<li><strong>Google Meet Link:</strong> <a href='{interview.GoogleMeetLink}' style='color: #2563eb; text-decoration: none;'>Join Meeting</a></li>"
                : "<li><strong>Meeting Link:</strong> Will be provided separately</li>";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #d97706; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        ul {{ padding-left: 0; list-style: none; }}
                        li {{ margin: 10px 0; padding: 8px; background: #f3f4f6; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 12px; }}
                        .update-notice {{ background: #fef3c7; padding: 15px; border-left: 4px solid #d97706; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Interview Updated 🔄</h2>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{interview.InterviewerName}</strong>,</p>
                            
                            <div class='update-notice'>
                                <strong>ℹ️ Notice:</strong> An interview has been updated. Please review the new details below.
                            </div>
                            
                            <div class='details'>
                                <h3>📋 Updated Interview Details:</h3>
                                <ul>
                                    <li><strong>Position:</strong> {interview.JobTitle}</li>
                                    <li><strong>Candidate:</strong> {interview.CandidateName}</li>
                                    <li><strong>Candidate Email:</strong> <a href='mailto:{interview.CandidateEmail}'>{interview.CandidateEmail}</a></li>
                                    <li><strong>Date & Time:</strong> {interview.StartTime:dddd, MMMM dd, yyyy} at {interview.StartTime:HH:mm} UTC</li>
                                    <li><strong>Duration:</strong> {(interview.EndTime - interview.StartTime).TotalMinutes} minutes</li>
                                    {meetLinkSection}
                                </ul>
                            </div>
                            
                            <p>Your calendar has been updated with the new details.</p>
                            
                            <div class='footer'>
                                <p>Best regards,<br><strong>Interview Scheduler Team</strong></p>
                                <p>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string CreateCandidateCancellationEmailBody(Interview interview)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #dc2626; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        ul {{ padding-left: 0; list-style: none; }}
                        li {{ margin: 10px 0; padding: 8px; background: #f3f4f6; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 12px; }}
                        .cancel-notice {{ background: #fef2f2; padding: 15px; border-left: 4px solid #dc2626; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Interview Cancelled ❌</h2>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{interview.CandidateName}</strong>,</p>
                            
                            <div class='cancel-notice'>
                                <strong>Notice:</strong> Your interview for the {interview.JobTitle} position has been cancelled.
                            </div>
                            
                            <div class='details'>
                                <h3>Cancelled Interview Details:</h3>
                                <ul>
                                    <li><strong>Position:</strong> {interview.JobTitle}</li>
                                    <li><strong>Originally Scheduled:</strong> {interview.StartTime:dddd, MMMM dd, yyyy} at {interview.StartTime:HH:mm} UTC</li>
                                    <li><strong>Interviewer:</strong> {interview.InterviewerName}</li>
                                </ul>
                            </div>
                            
                            <p>We apologize for any inconvenience this may cause. If you have any questions or if this cancellation was made in error, please contact us directly.</p>
                            
                            <div class='footer'>
                                <p>Best regards,<br><strong>Interview Scheduler Team</strong></p>
                                <p>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string CreateInterviewerCancellationEmailBody(Interview interview)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #dc2626; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9fafb; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        ul {{ padding-left: 0; list-style: none; }}
                        li {{ margin: 10px 0; padding: 8px; background: #f3f4f6; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 12px; }}
                        .cancel-notice {{ background: #fef2f2; padding: 15px; border-left: 4px solid #dc2626; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Interview Cancelled ❌</h2>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{interview.InterviewerName}</strong>,</p>
                            
                            <div class='cancel-notice'>
                                <strong>Notice:</strong> An interview has been cancelled.
                            </div>
                            
                            <div class='details'>
                                <h3>Cancelled Interview Details:</h3>
                                <ul>
                                    <li><strong>Position:</strong> {interview.JobTitle}</li>
                                    <li><strong>Candidate:</strong> {interview.CandidateName}</li>
                                    <li><strong>Originally Scheduled:</strong> {interview.StartTime:dddd, MMMM dd, yyyy} at {interview.StartTime:HH:mm} UTC</li>
                                </ul>
                            </div>
                            
                            <p>The interview has been removed from your calendar and the candidate has been notified.</p>
                            
                            <div class='footer'>
                                <p>Best regards,<br><strong>Interview Scheduler Team</strong></p>
                                <p>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}