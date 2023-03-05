using System.Net;
using dotenv.net;
using Kjeholmen.Services;
using Kjeholmen.Services.Api;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;

namespace Kjeholmen.UnitTests;

public class TestPollService
{
    private EmailServiceOptions _serviceOptions = new("dummy");
    private SmsServiceOptions _smsServiceOptions = new("dummySid", "DummyAuth", "dummyphone");
    private PollService _pollService;
    private ApiClient _client;

    public TestPollService()
    {
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false, probeForEnv: true));

        var username = Environment.GetEnvironmentVariable("USER_NAME_OSLOFJORD") ??
                        throw new Exception("Username not set!");
        var password = Environment.GetEnvironmentVariable("PASSWORD_OSLOFJORD") ??
                        throw new Exception("Password not set!");

        _client = new ApiClient(username, password);

        _pollService = new PollService(_client, _serviceOptions, _smsServiceOptions);
    }

    [Fact]
    public void TestAuthentication()
    {
        var response = _client.GetToken();
        Assert.True(response.Result.StatusCode == HttpStatusCode.OK, $"Response: {response.Result.StatusCode}");
    }
}