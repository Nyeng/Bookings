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
        
        var emailApiKey = Environment.GetEnvironmentVariable("EMAIL_API_KEY") ??
                          throw new Exception("Email API Key not set!");
        
        var twilioSid = Environment.GetEnvironmentVariable("TWILIO_SID") ??
                        throw new Exception("TWILIO_SID not set!");
        
        var twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ??
                              throw new Exception("TWILIO_SID not set!");
        
        var twilioPhoneNumber = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ??
                                throw new Exception("TWILIO_PHONE_NUMBER not set!");
        
        var username = Environment.GetEnvironmentVariable("USER_NAME_OSLOFJORD") ??
                       throw new Exception("Username not set!");
        
        var password = Environment.GetEnvironmentVariable("PASSWORD_OSLOFJORD") ??
                       throw new Exception("Password not set!");

        var apiClient = new ApiClient(username,password);
        IEmailServiceOptions emailOptions = new EmailServiceOptions(emailApiKey);

        ISmsServiceOptions smsOptions = new SmsServiceOptions(twilioSid, twilioAuthToken, twilioPhoneNumber);
        var smSservice = new SmsService(smsOptions);

        _pollService = new PollService(apiClient, emailOptions, smsOptions);
        
        var phoneOne = Environment.GetEnvironmentVariable("PHONE_RECIPIENT_ONE") ??
                       throw new Exception("Phone number not set for first recipient");

        var phoneTwo = Environment.GetEnvironmentVariable("PHONE_RECIPIENT_TWO") ??
                       throw new Exception("Phone number not set for second receiver");
        
        var fullName = Environment.GetEnvironmentVariable("FULL_NAME") ??
                       throw new Exception("Phone number not set for second receiver");
        

        await _pollService.PollOsloFjorden(phoneOne, phoneTwo,fullName);


        var resp = await smSservice.SendSms("Program stopped for some reason! You might want to check it out",
            phoneOne
        );
        Console.WriteLine(resp.Body + "\n" + resp.Status);
    }
}