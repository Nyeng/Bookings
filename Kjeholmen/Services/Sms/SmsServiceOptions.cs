namespace Kjeholmen.Services.Sms;

public class SmsServiceOptions : ISmsServiceOptions
{
    public string TwilioSid { get; }
    public string TwilioAuthToken { get; }
    public string PhoneRecipientOne { get; }
    public string PhoneNumberTwilio { get; }
    public string PhoneRecipientTwo { get; }

    public SmsServiceOptions(string twilioSid, string twilioAuthToken)
    {
        TwilioAuthToken = twilioAuthToken;
        TwilioSid = twilioSid;
    }
}