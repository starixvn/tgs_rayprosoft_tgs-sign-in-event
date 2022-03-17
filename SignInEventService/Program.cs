using Microsoft.EntityFrameworkCore;
using SignInEventService.MessageQueue;
using Tgs.SignInEvents.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddSignInEvents()
	.AddSignInEventRepositories((sp, dbBuilder) =>
	{
		var config = sp.GetRequiredService<IConfiguration>();

		dbBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
	})
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfiguration => natsConfiguration
		.ConfigQueueOptions((options, sp) =>
		{
			var config = sp.GetRequiredService<IConfiguration>();

			options.Url = config.GetConnectionString("Nats");
		})
		.AddProcessor<SignInProcessor>("tgs.signin.daily", "tgs.signin.daily")
		.AddProcessor<SignInQueryProcessor>("tgs.signin.daily.query", "tgs.signin.daily.query"))
	.AddNatsGlobPatternExchange("*");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
