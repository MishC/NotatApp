using Microsoft.AspNetCore.Mvc;
using NotatApp.Services; // kde máš IEmailSender

namespace NotatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public TestEmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        // POST /api/testemail
        [HttpPost]
        public async Task<IActionResult> SendTest([FromBody] TestEmailRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.To))
                return BadRequest(new { message = "Missing 'to' email address" });

            var subject = string.IsNullOrWhiteSpace(dto.Subject)
                ? "Test email from NoteApp / SES"
                : dto.Subject;

            var body = string.IsNullOrWhiteSpace(dto.Body)
                ? "Hello! This is a test email from your NoteApp backend using Amazon SES."
                : dto.Body;

            await _emailSender.SendAsync(dto.To, subject, body);

            return Ok(new
            {
                message = "If SES is configured correctly, the email should be on its way.",
                to = dto.To
            });
        }
    }

    public class TestEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}
