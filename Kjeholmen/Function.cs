namespace Kjeholmen;

public class Function
{
    FunctionName("PollOsloFjorden")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        // Retrieve environment variables

        // Create dependencies

        // Create PollService instance using dependencies
        IPollService pollService = new PollService(apiClient, emailOptions, smsOptions);

        // Call PollOsloFjorden method
        await pollService.PollOsloFjorden(phoneOne, phoneTwo, fullName);

        // Send SMS message

        return new OkResult();
    }
}