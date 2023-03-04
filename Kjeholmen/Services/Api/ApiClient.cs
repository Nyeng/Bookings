using System.Text;

namespace Kjeholmen.Services.Api;

public class ApiClient
{
    public readonly HttpClient HttpClient;

    public readonly string Username;
    public readonly string Password;

    public ApiClient(string username, string password)
    {
        HttpClient = new HttpClient();
        HttpClient = SetupHttpClient();

        Username = username;
        Password = password;
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

        var requestBody = $"{{\"email\":\"{Username}\",\"password\":\"{Password}\"}}";

        //Login
        var tokenresponse = HttpClient.PostAsync("api/user/token/",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));

        return tokenresponse;
    }
}