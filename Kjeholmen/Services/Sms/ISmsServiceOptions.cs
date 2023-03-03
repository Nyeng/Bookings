namespace Kjeholmen.Services;

public interface ISmsServiceOptions
{
    string TwilioSid { get; }
    string TwilioAuthToken { get; }

    string PhoneRecipientOne { get; }

    string PhoneNumberTwilio { get; }

    string PhoneRecipientTwo { get; }
}