using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignInEventService.Protos;
using Tgs.SignInEvents.Clients;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services => services
	.AddSignInClient()
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, sp) =>
		{
		}))
	.AddNatsGlobPatternExchange("*"));

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var playerCode = "4ucmqplx7okb";

var signInClient = app.Services.GetRequiredService<ISignInClient>();

async ValueTask SignInAsync(Guid promotionId, DateOnly date)
{
	var request = new SignInRequest
	{
		PromotionId = promotionId,
		PlayerCode = playerCode,
		Date = date
	};

	var response = await signInClient
		.SignInAsync(promotionId, playerCode, date)
		.ConfigureAwait(false);

	Console.WriteLine(response.Combo);
}

async ValueTask QuerySignInAsync(Guid promotionId, DateOnly date)
{
	var response = await signInClient
		.GetSignInRecordAsync(promotionId, playerCode, date)
		.ConfigureAwait(false);

	Console.WriteLine($"{response?.Id}, {response?.Combo}");
}

var promotionId = Guid.Parse("3619ff68-2ff8-45b6-90bc-1465a1ec8cc4");
var date = DateOnly.FromDateTime(DateTime.Now);

await SignInAsync(promotionId, date).ConfigureAwait(false);
await SignInAsync(promotionId, date.AddDays(1)).ConfigureAwait(false);
await SignInAsync(promotionId, date.AddDays(2)).ConfigureAwait(false);

await QuerySignInAsync(promotionId, date).ConfigureAwait(false);
await QuerySignInAsync(promotionId, date.AddDays(5)).ConfigureAwait(false);
