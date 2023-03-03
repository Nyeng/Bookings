// See https://aka.ms/new-console-template for more information

namespace Kjeholmen;

public class Program
{
    private static PollService _pollService;

    public static async Task Main()
    {
        _pollService = new PollService();
        await _pollService.PollOsloFjorden();
    }
}