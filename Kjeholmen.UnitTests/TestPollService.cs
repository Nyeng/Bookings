using System.Net;
using dotenv.net;
using Kjeholmen.Services;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;

namespace Kjeholmen.UnitTests;

public class TestPollService
{
    private ApiClient _client = new();
    private EmailServiceOptions _serviceOptions = new("dummy");
    private SmsServiceOptions _smsServiceOptions = new("dummySid", "DummyAuth");
    private PollService _pollService;

    public TestPollService()
    {
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));
        _pollService = new PollService(_client, _serviceOptions, _smsServiceOptions);
    }

    [Fact]
    public void TestAuthentication()
    {
        var response = _client.GetToken();
        Assert.True(response.Result.StatusCode == HttpStatusCode.OK, $"Response: {response.Result.StatusCode}");
        
    }
}