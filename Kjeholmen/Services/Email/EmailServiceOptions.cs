namespace Kjeholmen.Services.Email;

public class EmailServiceOptions : IEmailServiceOptions
{
    public string ApiKey { get; }

    public EmailServiceOptions(string apiKey)
    {
        ApiKey = apiKey;
    }
}