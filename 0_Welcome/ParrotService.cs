using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Welcome;

public class ParrotService(IOptions<Message> message, Channel<string> channel, ILogger<ParrotService> logger) : BackgroundService
{
    string? _message;
    Task? _parrot;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _message = message.Value.Text;
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
            await _parrot;
        }

        return;

        async Task Action()
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_message is not null)
                {
                    logger.Say(DateTime.Now, _message);
                }

                await Task.Delay(2000, stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _parrot?.Dispose();
    }
}

public static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{On:HH:mm:ss} | Parrot says: {Message}")]
    public static partial void Say(this ILogger logger, DateTime on, string message);
}
