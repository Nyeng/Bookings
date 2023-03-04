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
    private readonly string _username;
    private readonly string _password;
    private ApiClient _client;

    public TestPollService()
    {
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));

        _username = Environment.GetEnvironmentVariable("USER_NAME_OSLOFJORD") ??
                    throw new Exception("Username not set!");
        _password = Environment.GetEnvironmentVariable("PASSWORD_OSLOFJORD") ??
                    throw new Exception("Password not set!");

        _client = new ApiClient(_username, _password);

        _pollService = new PollService(_client, _serviceOptions, _smsServiceOptions);
    }

    [Fact]
    public void TestAuthentication()
    {
        var response = _client.GetToken();
        Assert.True(response.Result.StatusCode == HttpStatusCode.OK, $"Response: {response.Result.StatusCode}");
    }
}