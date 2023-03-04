namespace Kjeholmen.Services.Sms;

public class SmsServiceOptions : ISmsServiceOptions
{
    public string TwilioSid { get; }
    public string TwilioAuthToken { get; }
    public string PhoneNumberTwilio { get; }

    public SmsServiceOptions(string twilioSid, string twilioAuthToken, string phoneNumberTwilio)
    {
        TwilioAuthToken = twilioAuthToken;
        PhoneNumberTwilio = phoneNumberTwilio;
        TwilioSid = twilioSid;
    }
}