using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Kjeholmen.Services;
using Kjeholmen.Services.Api;
using Kjeholmen.Services.Email;
using Kjeholmen.Services.Sms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kjeholmen;

public class PollService
{
    private readonly ApiClient _client;
    private string _infoText;
    private readonly EmailService _emailService;
    private readonly SmsService _smsService;
    public readonly ISmsServiceOptions SmsOptions;

    public PollService(ApiClient client, IEmailServiceOptions emailServiceOptions, ISmsServiceOptions smsServiceOptions)
    {
        SmsOptions = smsServiceOptions;
        _infoText = "";
        _client = client;
        _emailService = new EmailService(emailServiceOptions);
        _smsService = new SmsService(SmsOptions);
    }

    public async Task PollOsloFjorden(string phoneOne, string phoneTwo, string fullName)
    {
        var date = DateTime.Now;
        var now = date.ToString("HH:mm:ss");

        Console.WriteLine("Starting poll service at: " + now);

        var bookingId = Guid.NewGuid().ToString();

        var requestPayload = GeneratePayLoad(bookingId, fullName, phoneTwo);

        await PollUntilBookingIsSuccessful(requestPayload, phoneOne, phoneTwo, bookingId);
    }

    private async Task PollUntilBookingIsSuccessful(string requestPayload, string phoneOne, string phoneTwo,
        string bookingId)
    {
        var statuskode = HttpStatusCode.BadRequest.GetHashCode();

        while (statuskode is not (200 or 201 or 202))
        {
            try
            {
                var currentTime = DateTime.Now;
                var formattedTime = currentTime.ToString("HH:mm:ss");
                Console.WriteLine($"{formattedTime} - Prøver å polle Kjeholmen API");

                var respons = _client.HttpClient.PostAsync("api/booking/",
                    new StringContent(requestPayload, Encoding.UTF8, "application/json"));

                var responsContent = respons.Result.Content.ReadAsStringAsync().Result;
                statuskode = respons.Result.StatusCode.GetHashCode();

                switch (statuskode)
                {
                    case 401:
                        await HandleUnauthorized();
                        break;
                    case 400:
                        await HandleBadRequest(responsContent, phoneOne);
                        //error still present, sleep a lil longer
                        Console.WriteLine("Fikk tilbakemelding: " + responsContent +
                                          "\nPrøver på nytt om 60 sekunder");

                        Thread.Sleep(60000);
                        break;
                    case > 199 and < 300:
                        await HandleSuccessfulBooking(bookingId, phoneOne, phoneTwo);
                        break;
                    default:
                        Console.WriteLine($"Ukjent feil: {statuskode}, sover i fem sekunder og prøver igjen");
                        Thread.Sleep(5000);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"failed with something {e.Message}");
            }
        }
    }

    private async Task HandleBadRequest(string responseContent, string phoneOne)
    {
        if (!responseContent.Contains("Booking is allowed only 48 hours"))
        {
            const string text =
                "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))";
            _infoText = text;
            Console.WriteLine(text);
            await _smsService.SendSms(_infoText, phoneOne);
        }
    }

    private async Task HandleSuccessfulBooking(string bookingId, string phoneOne, string phoneTwo)
    {
        var directFile = await File.ReadAllTextAsync("Requests/direct.json");
        directFile = directFile.Replace("{id}", Guid.NewGuid().ToString());
        directFile = directFile.Replace("{bookingId}", bookingId);

        Console.WriteLine("Prøver direct request med: " + directFile);

        var directResponse = await _client.HttpClient.PostAsync("api/payment/direct",
            new StringContent(directFile, Encoding.UTF8, "application/json"));

        Console.WriteLine("Api payment direct response" + directResponse.StatusCode);

        if (directResponse.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.OK or HttpStatusCode.Created)
        {
            Console.WriteLine(directResponse.Content.ReadAsStringAsync().Result);

            var obj = JsonConvert.DeserializeObject<JObject>(directResponse.Content.ReadAsStringAsync().Result);
            var providerPaymentId = (string)obj.SelectToken("providerPaymentId");

            var url =
                $"https://friluftsliv.oslofjorden.org/payment?paymentId={providerPaymentId}&serviceId={bookingId}&type=booking";

            var text =
                "Forsøkte å booke Kjeholmen! Følg betalings-url og fullfør bookingen din på denne lenken: (TRYGG LENKE FRA VEGARD): (hvis det feiler bare prøv å book hytta på nytt, den er forhåpentligvis tilgjengelig) \n\nURL: "
                + url;

            Console.WriteLine(text);

            try
            {
                var smsTask = _smsService.SendSms(text, phoneOne);
                var smsTaskTwo = _smsService.SendSms(text, phoneTwo);
                var emailTask = _emailService.SendEmailNotification();

                Task.WhenAll(smsTask, smsTaskTwo, emailTask).Wait();

                var emailResponse = smsTask.Result;
                var smsResponse = emailTask.Result;

                Console.WriteLine(
                    $"Email response: {emailResponse.Status} \nSms resp: {smsResponse.StatusCode} {smsResponse.Body} ");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending sms or email: " + e.Message);
            }
        }
    }

    private async Task HandleUnauthorized()
    {
        Console.WriteLine("Henter nytt token");
        var resp = await _client.GetToken();

        if (resp.StatusCode != HttpStatusCode.OK)
            throw new Exception("Unable to login using the current credentials");

        var tokenContent = resp.Content.ReadAsStringAsync().Result;
        var responseObj = JObject.Parse(tokenContent);
        var accessToken = (string)responseObj["access_token"];

        _client.HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private string GeneratePayLoad(string bookingId, string fullName, string phoneTwo)
    {
        return
            $"{{\"id\":\"{bookingId}\",\"isManual\":false,\"bookingCategory\":\"Annet\",\"numberOfAdult\":10,\"numberOfChildren\":2,\"checkInDate\":" +
            $"\"2023-06-16\",\"checkOutDate\":\"2023-06-18\",\"cabin\":{{\"title\":\"Kjeholmen\",\"defaultImageUrl\":\"https://oslofjorden.imgix.net/cabin/d8ce52d8-ad08-4d61-afad-e78e7949d3a3/852cf2cb-9426-454d-acf8-3ecfc1f6bfa6.jpg\"," +
            $"\"id\":\"d8ce52d8-ad08-4d61-afad-e78e7949d3a3\"}},\"user\":{{\"id\":\"1e7bb797-15bf-4c24-ad04-f303b10cc8b3\",\"email\":\"{_client.Username}\",\"name\":\"{fullName}\",\"phoneNumber\":\"{phoneTwo}\"}},\"" +
            $"price\":{{\"currency\":\"NOK\",\"comment\":\"Summary: 2 days x 3000NOK/day = 6000NOK\",\"price\":6000,\"discount\":0,\"total\":6000}}}}";
    }
}