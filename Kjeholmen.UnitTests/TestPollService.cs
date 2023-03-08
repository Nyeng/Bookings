using System.Net;
using System.Net.Http.Headers;
using System.Text;
using dotenv.net;
using Kjeholmen.Services.Api;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace Kjeholmen.UnitTests;

public class TestPollService
{
    private EmailServiceOptions _serviceOptions = new("dummy");
    private SmsServiceOptions _smsServiceOptions = new("dummySid", "DummyAuth", "dummyphone");
    private readonly ApiClient _client;
    private readonly ITestOutputHelper _testOutputHelper;

    public TestPollService(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        DotEnv.Load();
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false, probeForEnv: true));

        var username = Environment.GetEnvironmentVariable("USER_NAME_OSLOFJORD") ??
                       throw new Exception("Username not set!");
        var password = Environment.GetEnvironmentVariable("PASSWORD_OSLOFJORD") ??
                       throw new Exception("Password not set!");

        _client = new ApiClient(username, password);
    }

    [Fact]
    public void TestAuthentication()
    {
        var response = _client.GetToken();
        Assert.True(response.Result.StatusCode == HttpStatusCode.OK, $"Response: {response.Result.StatusCode}");
    }


    [Fact]
    public async Task CheckAvailability()
    {
        var resp = await _client.HttpClient.GetAsync("api/cabin/d8ce52d8-ad08-4d61-afad-e78e7949d3a3");

        _testOutputHelper.WriteLine(resp.StatusCode.ToString());

        var avilabliliaty = await _client.HttpClient.GetAsync(
            "api/cabin/kjeholmen/?includeSupervisors=false&includeAccounting=false&includeReviews=false&includeMedia=false");


        Assert.True(avilabliliaty.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoadFileTest()
    {
        var test = await File.ReadAllTextAsync("Requests/direct.json");

        _testOutputHelper.WriteLine(test);

        var exception = await Record.ExceptionAsync(() => File.ReadAllTextAsync("Requests/direct.json"));

        //Assert
        Assert.Null(exception);
    }
}