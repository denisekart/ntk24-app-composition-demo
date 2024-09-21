using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.AddSimpleConsole(opts => opts.SingleLine = true);
builder.Services.AddSingleton(_ => Channel.CreateUnbounded<string?>());
builder.Services.AddOptions<Message>().BindConfiguration("Message");
builder.Services.AddHostedService<EchoService>();

var app = builder.Build();

app.MapGet("echo", async ([FromQuery(Name = "t")] string? text, Channel<string> channel) => { await channel.Writer.WriteAsync(text); });

var channelWriter = app.Services.GetRequiredService<Channel<string?>>().Writer;
var options = app.Services.GetRequiredService<IOptionsMonitor<Message>>();

await channelWriter.WriteAsync(options.CurrentValue.Text);
options.OnChange(async current => { await channelWriter.WriteAsync(current.Text); });

app.Run();