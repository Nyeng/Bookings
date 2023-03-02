namespace Kjeholmen;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

// using SendGrid's C# Library
// https://github.com/sendgrid/sendgrid-csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class Kjeholmen
{
    public const string url = "https://api.oslofjorden.org/";
    private static string infoText = "";

    const string requestPayload =
        "{\"id\":\"0ab32230-445c-45b8-b76c-a84b44b291d3\",\"isManual\":false,\"bookingCategory\":\"Barnefamilie\",\"numberOfAdult\":10,\"numberOfChildren\":2,\"checkInDate\":\"2023-06-16\",\"checkOutDate\":\"2023-06-18\",\"cabin\":{\"title\":\"Kjeholmen\",\"defaultImageUrl\":\"https://oslofjorden.imgix.net/cabin/d8ce52d8-ad08-4d61-afad-e78e7949d3a3/852cf2cb-9426-454d-acf8-3ecfc1f6bfa6.jpg\",\"id\":\"d8ce52d8-ad08-4d61-afad-e78e7949d3a3\"},\"user\":{\"id\":\"1e7bb797-15bf-4c24-ad04-f303b10cc8b3\",\"email\":\"emma.j.lennox@gmail.com\",\"name\":\"Emma Lennox\",\"phoneNumber\":\"+4790267664\"},\"price\":{\"currency\":\"NOK\",\"comment\":\"Summary: 2 days x 3000NOK/day = 6000NOK\",\"price\":6000,\"discount\":0,\"total\":6000}}";

    public static HttpClient client;


    public static void Main(string[] args)
    {
        client = new HttpClient();
        const string token = "invalidtoken";
        client.DefaultRequestHeaders.Add("Authorization",
            "Bearer " + token);
        client.BaseAddress = new Uri(url);
    }


    public static async Task PollOsloFjorden()
    {
        var statuskode = HttpStatusCode.BadRequest.GetHashCode();
        while (statuskode is not (200 or 201 or 202))
        {
            // break;
            var pollingRateInMinutes = 1000 * 60;

            var respons = client.PostAsync("api/booking/",
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
                    infoText =
                        "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))";
                    Console.WriteLine(
                        "Exit loop, something's fishy or doesn't work! (New 400 bad request exception, notify Vegard))");
                    break;
                }
            }

            if (statuskode == 401)
            {
                Console.WriteLine("Henter nytt token");
                //Fetch new token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                //Don't sleep when logged out
                pollingRateInMinutes = 100;
            }

            else if (statuskode is > 199 and < 300)
            {
                Console.WriteLine("Booking completed:) " + responsContent);

                try
                {
                    SendSms(responsContent);
                    await SendEmailNotification();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending sms or email: " + e.Message);
                }

                break;
            }

            Thread.Sleep(pollingRateInMinutes);
        }
    }


    public static void SendSms(string body)
    {
        //if body is empty set default body
        var smsbody =
            "Din booking til Kjeholmen er *kaanskje* gått gjennom - sjekk profilen din på https://www.oslofjorden.org/ Stopper script inntil videre\n"
            + "Respons fra server:" + body;

        var accountSid = Environment.GetEnvironmentVariable("TWILIO_SID");
        var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        TwilioClient.Init(accountSid, authToken);

        var message = MessageResource.Create(
            body: smsbody,
            from: new Twilio.Types.PhoneNumber("+12765215689"),
            to: new Twilio.Types.PhoneNumber("+4797634078")
        );

        Console.WriteLine(message.Sid);

        var messageEmma = MessageResource.Create(
            body: smsbody,
            from: new Twilio.Types.PhoneNumber("+12765215689"),
            to: new Twilio.Types.PhoneNumber("+4790267664")
        );

        Console.WriteLine(messageEmma.Sid);
    }

    public static void VarsleVegard()
    {
        Console.WriteLine("Loop stopet, job stoppt, varsler Vegard på SMS");
        SendSmsVegard(infoText);
    }


    static void SendSmsVegard(string loopStopetJobStopptVarslerVegardPåSms)
    {
        var smsBody = "Kjeholmenscriptet stantset: \n" + loopStopetJobStopptVarslerVegardPåSms;

        var accountSid = Environment.GetEnvironmentVariable("TWILIO_SID");
        var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        TwilioClient.Init(accountSid, authToken);

        var message = MessageResource.Create(
            body: smsBody,
            from: new Twilio.Types.PhoneNumber("+12765215689"),
            to: new Twilio.Types.PhoneNumber("+4797634078")
        );
        Console.WriteLine(message.Sid);
    }

    static string GetToken()
    {
        Console.WriteLine("Ikke autentisert, logger inn på nytt");
        const string requestBody = "{\"email\":\"emma.j.lennox@gmail.com\",\"password\":\"Psykinst1993!\"}";
        //Login
        var tokenresponse = client.PostAsync("api/user/token/",
            new StringContent(requestBody, Encoding.UTF8, "application/json")).Result;

        if (tokenresponse.StatusCode != HttpStatusCode.OK)
            throw new Exception("Could not get token");

        var tokenContent = tokenresponse.Content.ReadAsStringAsync().Result;
        var responseObj = JObject.Parse(tokenContent);
        var access_token = (string)responseObj["access_token"];

        return access_token;
    }

    static async Task SendEmailNotification()
    {
        var apiKey = Environment.GetEnvironmentVariable("EMAIL_GRID_TOKEN");

        var sendGridClient = new SendGridClient(apiKey);
        var from = new EmailAddress("vegard.nyeng@gmail.com", "Vegard Nyeng");
        const string subject = "Angående din booking til Kjeholmen";
        var to = new EmailAddress("vegard.nyeng@gmail.com", "Vegard");
        const string plainTextContent = "Din booking til Kjeholmen er gått gjennom :)";
        const string htmlContent = "<strong>Sjekk ut bookingen på nettsiden: https://www.oslofjorden.org/ </strong>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await sendGridClient.SendEmailAsync(msg);
        Console.WriteLine("Email sent" + response.StatusCode + response.Body + response);
    }
}