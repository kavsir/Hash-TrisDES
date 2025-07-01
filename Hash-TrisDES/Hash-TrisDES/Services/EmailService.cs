
using System.Net;
using System.Net.Mail;
namespace Hash_TrisDES.Services;
public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MailMessage();
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        // Cấu hình SMTP
        message.From = new MailAddress("dinhtruongphong2752004@gmail.com");
        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("dinhtruongphong2752004@gmail.com", "nwbmdgflyflicwpa"),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }
}
