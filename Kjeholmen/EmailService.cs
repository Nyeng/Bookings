using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kjeholmen;

public class EmailService
{
    public static async Task SendEmailNotification()
    {
        var apiKey = Environment.GetEnvironmentVariable("EMAIL_GRID_TOKEN");

        var sendGridClient = new SendGridClient(apiKey);
        var from = new EmailAddress("vegard.nyeng@gmail.com", "Vegard Nyeng");
        const string subject = "Angående din booking til Kjeholmen";
        var to = new EmailAddress("vegard.nyeng@gmail.com", "Vegard");
        const string plainTextContent = "Din booking til Kjeholmen er gått gjennom :)";
        const string htmlContent = "<strong>Sjekk ut bookingen på nettsiden: https://www.oslofjorden.org/ </strong>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await sendGridClient.SendEmailAsync(msg);
        Console.WriteLine("Email sent" + response.StatusCode + response.Body + response);
    }
}