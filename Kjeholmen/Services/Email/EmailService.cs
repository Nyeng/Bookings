using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kjeholmen.Services.Email;

public class EmailService
{
    private readonly SendGridClient _sendGridClient;
    
    public EmailService(IEmailServiceOptions options)
    {
        _sendGridClient = new SendGridClient(options.ApiKey);
    }

    public async Task<Response> SendEmailNotification()
    {
        const string email = "vegard.nyeng@gmail.com";

        var from = new EmailAddress("vegard.nyeng@gmail.com", "Vegard Nyeng");
        const string subject = "Angående din booking til Kjeholmen";
        var to = new EmailAddress(email, "Vegard");
        const string plainTextContent = "Din booking til Kjeholmen er gått gjennom :)";
        const string htmlContent = "<strong>Sjekk ut bookingen på nettsiden: https://www.oslofjorden.org/ </strong>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        
        // Console.WriteLine("Email sent" + response.StatusCode + response.Body + response);
        return await _sendGridClient.SendEmailAsync(msg);

    }
}