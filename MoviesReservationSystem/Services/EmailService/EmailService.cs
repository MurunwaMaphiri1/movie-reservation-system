using DotNetEnv;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace MoviesReservationSystem.Services.EmailService
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MagicBox Theatres", 
                Env.GetString("EmailAddress")));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync(Env.GetString("EmailAddress"), 
                    Env.GetString("GeneratedPassword"));
                await client.SendAsync(message);
                client.DisconnectAsync(true);
            }
        }
    }
}

