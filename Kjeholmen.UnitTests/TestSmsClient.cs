using dotenv.net;
using Kjeholmen.Services.Sms;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Xunit.Abstractions;

namespace Kjeholmen.UnitTests;

public class TestSmsClient
{
    private readonly ITestOutputHelper _testOutputHelper;
    public TestSmsClient(ITestOutputHelper testOutputHelper)
    {
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));

        _testOutputHelper = testOutputHelper;
        var root = Directory.GetCurrentDirectory();
    }

    [Fact]
    public async Task sendDummySms()
    {
        try
        {
            var smsService = new SmsService(new SmsServiceOptions("dummy", "dummyAuth", "duiummy"));
            await smsService.SendSms("Hei", "dummyReceiver");
        }
        catch (ApiException e)
        {
            _testOutputHelper.WriteLine("Invalid username expected: " + e);
        }
    }
}