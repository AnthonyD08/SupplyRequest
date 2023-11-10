using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Repositories;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace WebApi.Controllers
{
    [Route("Email")]
    public class EmailController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SendEmail([FromBody] Email email)
        {
            var success = await SendAsync(email);

            if (success)
                return Ok("Email sent successfully");

            return BadRequest("There was an error sending the email");
        }

        [HttpPost]
        [Route("SendTemplate")]
        public async Task<IActionResult> SendTemplate(int templateId, int requestId)
        {
            var requestRepo = new RequestRepository();

            var request = await requestRepo.ReadAsync(new Request() { RequestId = requestId });

            if (request is null)
                return BadRequest("Request doesn't exist");

            var userRepo = new UserRepository();

            var user = await userRepo.ReadAsync(new User() { UserId = request.Employee });

            if (user is null)
                return BadRequest("El usuario de este request no existe");

            string template;

            switch (templateId)
            {
                case 1:
                    template = EmailTemplates.RequestConfirmation(user.Name, request.RequestId);
                    break;
                case 2:
                    template = EmailTemplates.BossApproved(user.Name, request.RequestId);
                    break;
                case 3:
                    template = EmailTemplates.BossRejected(user.Name, request.RequestId);
                    break;
                case 4:
                    template = EmailTemplates.AccountantApproved(user.Name, request.RequestId);
                    break;
                case 5:
                    template = EmailTemplates.AccountantRejected(user.Name, request.RequestId);
                    break;
                case 6:
                    template = EmailTemplates.UserCancelled(user.Name, request.RequestId);
                    break;
                default:
                    return BadRequest("Invalid template id");
            }

            var email = new Email
            {
                Name = user.Name,
                EmailAddress = user.Email,
                Title = "Solicitud de suministros",
                Body = template
            };

            var success = await SendAsync(email);

            if (success)
                return Ok("Email sent successfully");

            return BadRequest("There was an error sending the email");
        }

        [NonAction]
        public async Task<bool> SendAsync(Email email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                "Supply Request",
                "ulacit378@gmail.com"
            ));

            message.To.Add(new MailboxAddress(
                $"{email.Name}",
                $"{email.EmailAddress}"
            ));

            message.Subject = email.Title;
            var bodyBuilder = new BodyBuilder
            {
                TextBody = email.Body
            };

            message.Body = bodyBuilder.ToMessageBody();

            var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.CheckCertificateRevocation = false;
            // Se conecta al servidor smtp de Google para enviar correos
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                userName: "ulacit378@gmail.com",
                password: "hqftvvuehhvuzdgk");

            try
            {
                _ = await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            await client.DisconnectAsync(true);

            return true;
        }
    }
}
