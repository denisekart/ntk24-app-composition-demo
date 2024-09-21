using System.Threading.Channels;

public class EchoService(Channel<string> channel, ILogger<EchoService> logger) : BackgroundService
{
    volatile string? _message;
    Task? _parrot;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async Task Action()
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.Say(_message);
                await Task.Delay(1000, stoppingToken);
            }
        }

        _parrot = Task.Run(Action, stoppingToken);
        try
        {
            while (await channel.Reader.ReadAsync(stoppingToken) is { } echo)
            {
                _message = echo;
            }
        }
        finally
        {
            _parrot?.Dispose();
        }
    }
}

public static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{Message}\t{Message}\\t{Message}\\t{Message}\\t{Message}")]
    public static partial void Say(this ILogger logger, string message);
}
