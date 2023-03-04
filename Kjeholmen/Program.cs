// See https://aka.ms/new-console-template for more information

using dotenv.net;
using Kjeholmen.Services;
using Kjeholmen.Services.Api;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;

namespace Kjeholmen;

public abstract class Program
{
    private static PollService? _pollService;


    public static async Task Main()
    {
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));

        // Retrieve environment variables
        var emailApiKey = GetRequiredEnvVariable("EMAIL_API_KEY");
        var twilioSid = GetRequiredEnvVariable("TWILIO_SID");
        var twilioAuthToken = GetRequiredEnvVariable("TWILIO_AUTH_TOKEN");
        var twilioPhoneNumber = GetRequiredEnvVariable("TWILIO_PHONE_NUMBER");
        var username = GetRequiredEnvVariable("USER_NAME_OSLOFJORD");
        var password = GetRequiredEnvVariable("PASSWORD_OSLOFJORD");
        var phoneOne = GetRequiredEnvVariable("PHONE_RECIPIENT_ONE");
        var phoneTwo = GetRequiredEnvVariable("PHONE_RECIPIENT_TWO");
        var fullName = GetRequiredEnvVariable("FULL_NAME");

        // Create dependencies
        var apiClient = new ApiClient(username, password);
        var emailOptions = new EmailServiceOptions(emailApiKey);
        var smsOptions = new SmsServiceOptions(twilioSid, twilioAuthToken, twilioPhoneNumber);
        var smsService = new SmsService(smsOptions);

        // Create PollService instance using dependencies
        _pollService = new PollService(apiClient, emailOptions, smsOptions);

        // Call PollOsloFjorden method
        await _pollService.PollOsloFjorden(phoneOne, phoneTwo, fullName);

        // Send SMS message
        var resp = await smsService.SendSms("Program stopped for some reason! You might want to check it out",
            phoneOne);
        Console.WriteLine(resp.Body + "\n" + resp.Status);
    }

    // Helper method to retrieve environment variables
    private static string GetRequiredEnvVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"Environment variable {variableName} not set!");
        }
        return value;
    }
}