using dotenv.net;
using Kjeholmen.Services.Sms;
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
    }

    [Fact]
    public async Task SendDummySms()
    {
        var smsService = new SmsService(new SmsServiceOptions("dummy", "dummyAuth", "duiummy"));

        var exception = await Record.ExceptionAsync(() => smsService.SendSms("Hei", "dummyReceiver"));
        
        Assert.NotNull(exception);

    }
}