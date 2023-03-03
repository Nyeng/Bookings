using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Kjeholmen.Services;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;
using Newtonsoft.Json.Linq;

namespace Kjeholmen;

public class PollService
{
    private readonly ApiClient _client;
    private string _infoText;
    private readonly EmailService _emailService;
    private readonly SmsService _smsService;

    public PollService(ApiClient client, IEmailServiceOptions emailServiceOptions, ISmsServiceOptions smsServiceOptions)
    {
        _infoText = "";
        _client = client;
        _emailService = new EmailService(emailServiceOptions);
        _smsService = new SmsService(smsServiceOptions);
    }

    public async Task PollOsloFjorden()
    {
        const string requestPayload =
            "{\"id\":\"0ab32230-445c-45b8-b76c-a84b44b291d3\",\"isManual\":false,\"bookingCategory\":\"Barnefamilie\",\"numberOfAdult\":10,\"numberOfChildren\":2,\"checkInDate\":\"2023-06-16\",\"checkOutDate\":\"2023-06-18\",\"cabin\":{\"title\":\"Kjeholmen\",\"defaultImageUrl\":\"https://oslofjorden.imgix.net/cabin/d8ce52d8-ad08-4d61-afad-e78e7949d3a3/852cf2cb-9426-454d-acf8-3ecfc1f6bfa6.jpg\",\"id\":\"d8ce52d8-ad08-4d61-afad-e78e7949d3a3\"},\"user\":{\"id\":\"1e7bb797-15bf-4c24-ad04-f303b10cc8b3\",\"email\":\"emma.j.lennox@gmail.com\",\"name\":\"Emma Lennox\",\"phoneNumber\":\"+4790267664\"},\"price\":{\"currency\":\"NOK\",\"comment\":\"Summary: 2 days x 3000NOK/day = 6000NOK\",\"price\":6000,\"discount\":0,\"total\":6000}}";

        var statuskode = HttpStatusCode.BadRequest.GetHashCode();

        while (statuskode is not (200 or 201 or 202))
        {
            // break;
            var pollingRateMilliseconds = 1000 * 60;

            try
            {
                var respons = _client.HttpClient.PostAsync("api/booking/",
                    new StringContent(requestPayload, Encoding.UTF8, "application/json"));

                var responsContent = respons.Result.Content.ReadAsStringAsync().Result;
                statuskode = respons.Result.StatusCode.GetHashCode();

                if (statuskode is >= 300 or < 200)
                    Console.WriteLine("Fikk statuskode " + statuskode + " og tilbakemelding: " + responsContent +
                                      "\nPrøver på nytt om " + pollingRateMilliseconds / 1000 / 60 + " minutt");

                if (statuskode is 400)
                {
                    if (!responsContent.Contains("Booking is allowed only 48 hours"))
                    {
                        _infoText =
                            "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))";
                        Console.WriteLine(
                            "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))");
                        await _smsService.SendSmsVegard(_infoText);
                        break;
                    }

                    //Keep looping
                }

                if (statuskode == 401)
                {
                    Console.WriteLine("Henter nytt token");
                    //Fetch new token
                    var resp = await _client.GetToken();

                    if (resp.StatusCode != HttpStatusCode.OK) 
                        throw new Exception("Unable to login using the current credentials");

                    var tokenContent = resp.Content.ReadAsStringAsync().Result;
                    var responseObj = JObject.Parse(tokenContent);
                    var accessToken = (string)responseObj["access_token"];

                    _client.HttpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    //Don't sleep when logged out
                    pollingRateMilliseconds = 100;
                }

                else if (statuskode is > 199 and < 300)
                {
                    Console.WriteLine("Booking completed:) " + responsContent);

                    try
                    {
                        var smsTask = _smsService.SendSmsEmmaVegard(responsContent);
                        var emailTask = _emailService.SendEmailNotification();

                        await Task.WhenAll(smsTask, emailTask);

                        var emailResponse = smsTask.Result;
                        var smsResponse = emailTask.Result;

                        Console.WriteLine(
                            $"Email and sms response: {emailResponse.message} \nsms resp: {smsResponse.StatusCode} {smsResponse.Body} ");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error sending sms or email: " + e.Message);
                    }

                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"failed with something {e.Message}");
            }

            Thread.Sleep(pollingRateMilliseconds);
        }
    }
}