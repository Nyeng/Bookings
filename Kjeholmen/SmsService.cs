using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Kjeholmen;

public class SmsService
{
    public static async Task SendSms(string body)
    {
        //if body is empty set default body
        var smsbody =
            "Din booking til Kjeholmen er *kaanskje* gått gjennom - sjekk profilen din på https://www.oslofjorden.org/ Stopper script inntil videre\n"
            + "Respons fra server:" + body;

        var accountSid = Environment.GetEnvironmentVariable("TWILIO_SID");
        var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        TwilioClient.Init(accountSid, authToken);

        var message = await MessageResource.CreateAsync(
            body: smsbody,
            from: new Twilio.Types.PhoneNumber("+12765215689"),
            to: new Twilio.Types.PhoneNumber("+4797634078")
        );

        var messageEmma = MessageResource.CreateAsync(
            body: smsbody,
            from: new Twilio.Types.PhoneNumber("+12765215689"),
            to: new Twilio.Types.PhoneNumber("+4790267664")
        );
    }

    public static async Task SendSmsVegard(string body)
    {
        //if body is empty set default body
        var accountSid = Environment.GetEnvironmentVariable("TWILIO_SID");
        var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        TwilioClient.Init(accountSid, authToken);

        var message = await MessageResource.CreateAsync(
            body: body,
            from: new Twilio.Types.PhoneNumber("+12765215689"),
            to: new Twilio.Types.PhoneNumber("+4797634078")
        );
        Console.WriteLine(message);
    }
}