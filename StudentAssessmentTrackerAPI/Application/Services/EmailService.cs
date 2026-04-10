using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>Contract for sending system emails.</summary>
    public interface IEmailService
    {
        /// <summary>Sends a plain-text email. Returns false if SMTP is not configured.</summary>
        Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody);

        /// <summary>Sends an assessment-created notification to a student.</summary>
        Task SendAssessmentCreatedAsync(
            string studentEmail,
            string studentName,
            string assessmentName,
            DateTime? dueDate,
            string? instructions);

        /// <summary>Sends a due-date reminder to a student.</summary>
        Task SendDueDateReminderAsync(
            string studentEmail,
            string studentName,
            string assessmentName,
            DateTime dueDate);
    }

    /// <summary>
    /// MailKit-backed email service. Silently no-ops when SMTP is not configured in
    /// appsettings so the application starts correctly without an email server.
    /// Configure the "Email" section in appsettings.json to enable real delivery.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        /// <summary>Initialises the service with app configuration and a logger.</summary>
        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var smtpHost = _config["Email:SmtpHost"];
            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogWarning("Email:SmtpHost is not configured — skipping email to {ToEmail}", toEmail);
                return false;
            }

            var port = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
            var username = _config["Email:Username"] ?? string.Empty;
            var password = _config["Email:Password"] ?? string.Empty;
            var fromEmail = _config["Email:FromEmail"] ?? username;
            var fromName = _config["Email:FromName"] ?? "Student Assessment Tracker";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, port, SecureSocketOptions.StartTls);
                if (!string.IsNullOrEmpty(username))
                    await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent to {ToEmail} — subject: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task SendAssessmentCreatedAsync(
            string studentEmail,
            string studentName,
            string assessmentName,
            DateTime? dueDate,
            string? instructions)
        {
            var dueLine = dueDate.HasValue
                ? $"<p><strong>Due Date:</strong> {dueDate.Value:dd MMMM yyyy}</p>"
                : string.Empty;
            var instrLine = !string.IsNullOrWhiteSpace(instructions)
                ? $"<p><strong>Instructions:</strong> {System.Net.WebUtility.HtmlEncode(instructions)}</p>"
                : string.Empty;

            var html = $"""
                <h2>New Assessment: {System.Net.WebUtility.HtmlEncode(assessmentName)}</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(studentName)},</p>
                <p>A new assessment has been assigned to you.</p>
                {dueLine}
                {instrLine}
                <p>Log in to your student portal to view your assessments.</p>
                <hr/>
                <small>Student Assessment Tracker — automated notification</small>
                """;

            await SendAsync(studentEmail, studentName,
                $"New Assessment Assigned: {assessmentName}", html);
        }

        /// <inheritdoc />
        public async Task SendDueDateReminderAsync(
            string studentEmail,
            string studentName,
            string assessmentName,
            DateTime dueDate)
        {
            var html = $"""
                <h2>Assessment Due Soon: {System.Net.WebUtility.HtmlEncode(assessmentName)}</h2>
                <p>Hi {System.Net.WebUtility.HtmlEncode(studentName)},</p>
                <p>Your assessment <strong>{System.Net.WebUtility.HtmlEncode(assessmentName)}</strong>
                   is due on <strong>{dueDate:dd MMMM yyyy}</strong>.</p>
                <p>Please log in to submit your work before the deadline.</p>
                <hr/>
                <small>Student Assessment Tracker — automated reminder</small>
                """;

            await SendAsync(studentEmail, studentName,
                $"Reminder: {assessmentName} is due {dueDate:dd MMM yyyy}", html);
        }
    }
}
