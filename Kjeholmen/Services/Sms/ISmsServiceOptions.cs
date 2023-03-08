namespace Kjeholmen.Services;

public interface ISmsServiceOptions
{
    string TwilioSid { get; }
    string TwilioAuthToken { get; }
    string PhoneNumberTwilio { get; }
}