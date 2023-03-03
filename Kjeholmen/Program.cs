// See https://aka.ms/new-console-template for more information

using dotenv.net;
using Kjeholmen.Services;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;

namespace Kjeholmen;

public abstract class Program
{
    private static PollService _pollService;

    public static async Task Main()
    {
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));

        //Env stuff:
        var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY") ??
                          throw new Exception("Email API Key not set!");

        var twilioSid = Environment.GetEnvironmentVariable("TWILIO_SID") ??
                        throw new Exception("TWILIO_SID not set!");

        var twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ??
                              throw new Exception("TWILIO_SID not set!");

        var apiClient = new ApiClient();
        IEmailServiceOptions emailOptions = new EmailServiceOptions(emailApiKey);

        ISmsServiceOptions smsOptions = new SmsServiceOptions(twilioSid, twilioAuthToken);
        var smSservice = new SmsService(smsOptions);

        _pollService = new PollService(apiClient, emailOptions, smsOptions);
        await _pollService.PollOsloFjorden();

        var resp = await smSservice.SendSmsVegard("Program stopped for some reason! You might want to check it out");
        Console.WriteLine(resp.Body + "\n" + resp.Status);
    }
}