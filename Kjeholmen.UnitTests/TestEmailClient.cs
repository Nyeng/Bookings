using System.Net;
using Kjeholmen.Services;
using Kjeholmen.Services.Email;
using SendGrid;
using Xunit.Abstractions;

namespace Kjeholmen.UnitTests;

public class TestEmailClient
{
    private EmailService _emailService;


    [Fact]
    public async Task TestClientWithInvalidApiKey()
    {
        _emailService = new EmailService(new EmailServiceOptions("dummykey"));
        var respons = await _emailService.SendEmailNotification();

        Assert.True(respons.StatusCode == HttpStatusCode.Unauthorized);
    }
}