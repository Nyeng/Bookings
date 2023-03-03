using System.Text;

namespace Kjeholmen.Services;

public class ApiClient
{
    public readonly HttpClient HttpClient;

    public ApiClient()
    {
        HttpClient = new HttpClient();
        HttpClient = SetupHttpClient();
    }

    private HttpClient SetupHttpClient()
    {
        HttpClient.Timeout = new TimeSpan(0, 0, 0, 30);
        const string token = "invalidtoken";
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        HttpClient.BaseAddress = new Uri("https://api.oslofjorden.org/");
        return HttpClient;
    }

    public Task<HttpResponseMessage> GetToken()
    {
        Console.WriteLine("Ikke autentisert, logger inn på nytt");

        var username = Environment.GetEnvironmentVariable("USER_NAME_OSLOFJORD") ??
                       throw new Exception("Username not set!");
        var password = Environment.GetEnvironmentVariable("PASSWORD_OSLOFJORD") ??
                       throw new Exception("Password not set!");

        var requestBody = $"{{\"email\":\"{username}\",\"password\":\"{password}\"}}";

        //Login
        var tokenresponse = HttpClient.PostAsync("api/user/token/",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));

        return tokenresponse;
    }
}