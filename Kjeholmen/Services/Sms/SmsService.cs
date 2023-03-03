using Twilio;
using Twilio.Clients;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;

namespace Kjeholmen.Services.Sms;

public class SmsService
{
    // Todo: add phone numbers here for now, but move out of this class:
    private readonly string _phoneTwilio;
    private readonly string _phoneOne;
    private readonly string _phoneTwo;

    public SmsService(ISmsServiceOptions options)
    {
        TwilioClient.Init(options.TwilioSid, options.TwilioAuthToken);

        _phoneTwilio = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ??
                       throw new Exception("Twilio nots et");
        _phoneOne = Environment.GetEnvironmentVariable("PHONE_RECIPIENT_ONE") ??
                    throw new Exception("Phone one");
        _phoneTwo = Environment.GetEnvironmentVariable("PHONE_RECIPIENT_TWO") ??
                    throw new Exception("Phone two");
    }

    public async Task<(MessageResource message, Task<MessageResource> messageEmma)> SendSmsEmmaVegard(string body)
    {
        //if body is empty set default body
        var smsbody =
            "Din booking til Kjeholmen er *kaanskje* gått gjennom - sjekk profilen din på https://www.oslofjorden.org/ Stopper script inntil videre\n"
            + "Respons fra server:" + body;

        var message = await MessageResource.CreateAsync(
            body: smsbody,
            from: new Twilio.Types.PhoneNumber(_phoneTwilio),
            to: new Twilio.Types.PhoneNumber(_phoneOne)
        );

        var messageEmma = MessageResource.CreateAsync(
            body: smsbody,
            from: new Twilio.Types.PhoneNumber(_phoneTwilio),
            to: new Twilio.Types.PhoneNumber(_phoneTwo)
        );

        return (message, messageEmma);
    }

    public async Task<MessageResource> SendSmsVegard(string body)
    {
        var message = await MessageResource.CreateAsync(
            body: body,
            from: new Twilio.Types.PhoneNumber(_phoneTwilio),
            to: new Twilio.Types.PhoneNumber(_phoneOne)
        );
        Console.WriteLine(message);


        return message;
    }
}