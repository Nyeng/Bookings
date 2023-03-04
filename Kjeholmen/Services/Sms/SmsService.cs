using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Kjeholmen.Services.Sms;

public class SmsService
{
    private readonly string _phoneTwilio;

    public SmsService(ISmsServiceOptions options)
    {
        TwilioClient.Init(options.TwilioSid, options.TwilioAuthToken);
        _phoneTwilio = options.PhoneNumberTwilio;
    }

    public async Task<MessageResource> SendSms(string body, string phoneNumber)
    {
        var message = await MessageResource.CreateAsync(
            body: body,
            from: new Twilio.Types.PhoneNumber(_phoneTwilio),
            to: new Twilio.Types.PhoneNumber(phoneNumber)
        );
        Console.WriteLine(message);
        return message;
    }
}