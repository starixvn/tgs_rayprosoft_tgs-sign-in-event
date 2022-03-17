using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignInEventService.Protos;
using Tgs.MessageQueue;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services => services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, sp) =>
		{
		}))
	.AddNatsGlobPatternExchange("*"));

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var playerCode = "4ucmqplx7okb";

var messageSender = app.Services.GetRequiredService<IMessageSender>();

async ValueTask SignInAsync(DateTime date)
{
	var request = new SignInRequest
	{
		PlayerCode = playerCode,
		Date = Timestamp.FromDateTime(date.ToUniversalTime())
	};

	var responseData = await messageSender!
		.RequestAsync("tgs.signin.daily", request.ToByteArray())
		.ConfigureAwait(false);
	var response = SignInResponse.Parser.ParseFrom(responseData.Span);

	Console.WriteLine(response.Combo);
}

async ValueTask QuerySignInAsync(DateTime date)
{
	var request = new SignInRequest
	{
		PlayerCode = playerCode,
		Date = Timestamp.FromDateTime(date.ToUniversalTime())
	};

	var responseData = await messageSender!
		.RequestAsync("tgs.signin.daily.query", request.ToByteArray())
		.ConfigureAwait(false);
	var response = SignInResponse.Parser.ParseFrom(responseData.Span);

	Console.WriteLine($"{response.Id}, {response.Combo}");
}

await SignInAsync(DateTime.UtcNow).ConfigureAwait(false);
await SignInAsync(DateTime.UtcNow.AddDays(1)).ConfigureAwait(false);
await SignInAsync(DateTime.UtcNow.AddDays(2)).ConfigureAwait(false);

await QuerySignInAsync(DateTime.UtcNow).ConfigureAwait(false);
await QuerySignInAsync(DateTime.UtcNow.AddDays(5)).ConfigureAwait(false);
