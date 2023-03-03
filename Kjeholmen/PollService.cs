using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Kjeholmen;

public class PollService
{
    private readonly HttpClient _client;
    private string _infoText;
    private readonly SmsService _smsService;
    private readonly EmailService _emailService;

    public PollService()
    {
        _infoText = "";
        _client = SetupHttpClient();
        _smsService = new SmsService();
        _emailService = new EmailService();
    }

    private static HttpClient SetupHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = new TimeSpan(0, 0, 0, 30);
        const string token = "invalidtoken";
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        httpClient.BaseAddress = new Uri("https://api.oslofjorden.org/");
        return httpClient;
    }

    public async Task PollOsloFjorden()
    {
        const string requestPayload =
            "{\"id\":\"0ab32230-445c-45b8-b76c-a84b44b291d3\",\"isManual\":false,\"bookingCategory\":\"Barnefamilie\",\"numberOfAdult\":10,\"numberOfChildren\":2,\"checkInDate\":\"2023-06-16\",\"checkOutDate\":\"2023-06-18\",\"cabin\":{\"title\":\"Kjeholmen\",\"defaultImageUrl\":\"https://oslofjorden.imgix.net/cabin/d8ce52d8-ad08-4d61-afad-e78e7949d3a3/852cf2cb-9426-454d-acf8-3ecfc1f6bfa6.jpg\",\"id\":\"d8ce52d8-ad08-4d61-afad-e78e7949d3a3\"},\"user\":{\"id\":\"1e7bb797-15bf-4c24-ad04-f303b10cc8b3\",\"email\":\"emma.j.lennox@gmail.com\",\"name\":\"Emma Lennox\",\"phoneNumber\":\"+4790267664\"},\"price\":{\"currency\":\"NOK\",\"comment\":\"Summary: 2 days x 3000NOK/day = 6000NOK\",\"price\":6000,\"discount\":0,\"total\":6000}}";

        var statuskode = HttpStatusCode.BadRequest.GetHashCode();

        while (statuskode is not (200 or 201 or 202))
        {
            // break;
            var pollingRateInMinutes = 1000 * 60;

            try
            {
                var respons = _client.PostAsync("api/booking/",
                    new StringContent(requestPayload, Encoding.UTF8, "application/json"));

                var responsContent = respons.Result.Content.ReadAsStringAsync().Result;
                statuskode = respons.Result.StatusCode.GetHashCode();

                if (statuskode is >= 300 or < 200)
                    Console.WriteLine("Fikk statuskode " + statuskode + " og tilbakemelding: " + responsContent +
                                      "\nPrøver på nytt om " + pollingRateInMinutes / 1000 / 60 + " minutt");

                if (statuskode is 400)
                {
                    if (!responsContent.Contains("Booking is allowed only 48 hours"))
                    {
                        _infoText =
                            "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))";
                        Console.WriteLine(
                            "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))");
                        await SmsService.SendSmsVegard(_infoText);
                        break;
                    }
                    
                    //Keep looping
                }

                if (statuskode == 401)
                {
                    Console.WriteLine("Henter nytt token");
                    //Fetch new token
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                    //Don't sleep when logged out
                    pollingRateInMinutes = 100;
                }

                else if (statuskode is > 199 and < 300)
                {
                    Console.WriteLine("Booking completed:) " + responsContent);

                    try
                    {
                        await SmsService.SendSms(responsContent);
                        await EmailService.SendEmailNotification();
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

            Thread.Sleep(pollingRateInMinutes);
        }
    }

    private string GetToken()
    {
        Console.WriteLine("Ikke autentisert, logger inn på nytt");
        const string requestBody = "{\"email\":\"emma.j.lennox@gmail.com\",\"password\":\"Psykinst1993!\"}";
        //Login
        var tokenresponse = _client.PostAsync("api/user/token/",
            new StringContent(requestBody, Encoding.UTF8, "application/json")).Result;

        if (tokenresponse.StatusCode != HttpStatusCode.OK)
            throw new Exception("Could not get token");

        var tokenContent = tokenresponse.Content.ReadAsStringAsync().Result;
        var responseObj = JObject.Parse(tokenContent);
        var accessToken = (string)responseObj["access_token"];

        return accessToken;
    }
}