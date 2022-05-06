using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SignInEventService.MessageQueue;
using Tgs.MessageQueue;
using Tgs.SignInEvents.Clients;
using Tgs.SignInEvents.Entities;

namespace SignInEvent.IntegrationTests;

[TestClass]
public class SignInEventTests
{
	[TestMethod]
	public async Task Get_SignInEvent_Record()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = new TestSignInEventApplication();

		var sut = new SignInClient(fakeMessageSender);

		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var date = DateOnly.FromDateTime(DateTime.Now);
		var playerCode = "tester";

		await using var serviceScope = web.Services.CreateAsyncScope();

		var dbContext = serviceScope.ServiceProvider.GetRequiredService<TgsContext>();

		_ = await dbContext.SignInEvents.AddAsync(new Tgs.SignInEvents.Entities.SignInEvent
		{
			Id = Guid.NewGuid(),
			BrandId = brandId,
			Date = date,
			PlayerCode = playerCode,
			Combo = 2,
			PromotionId = promotionId,
			CreatedOn = DateTime.Now
		});

		_ = await dbContext.SaveChangesAsync();

		_ = fakeMessageSender.RequestAsync(
			Arg.Is("tgs.signin.daily.query"),
			Arg.Any<ReadOnlyMemory<byte>>())
			.Returns(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var processor = ActivatorUtilities.CreateInstance<SignInQueryProcessor>(serviceScope.ServiceProvider);

				return processor.HandleAsync(data).Result;
			});

		var actual = await sut.GetSignInRecordAsync(promotionId, playerCode, date);

		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual?.Combo);
	}

	[TestMethod]
	public async Task Get_SignInEvent_Record_Not_Found_Will_Return_Null()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = new TestSignInEventApplication();

		var sut = new SignInClient(fakeMessageSender);

		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var date = DateOnly.FromDateTime(DateTime.Now);
		var playerCode = "tester";

		await using var serviceScope = web.Services.CreateAsyncScope();

		var dbContext = serviceScope.ServiceProvider.GetRequiredService<TgsContext>();

		_ = fakeMessageSender.RequestAsync(
			Arg.Is("tgs.signin.daily.query"),
			Arg.Any<ReadOnlyMemory<byte>>())
			.Returns(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var processor = ActivatorUtilities.CreateInstance<SignInQueryProcessor>(serviceScope.ServiceProvider);

				return processor.HandleAsync(data).Result;
			});

		var actual = await sut.GetSignInRecordAsync(promotionId, playerCode, date);

		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task SignIn()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = new TestSignInEventApplication();

		var sut = new SignInClient(fakeMessageSender);

		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var date = DateOnly.FromDateTime(DateTime.Now);
		var playerCode = "tester";

		await using var serviceScope = web.Services.CreateAsyncScope();

		var dbContext = serviceScope.ServiceProvider.GetRequiredService<TgsContext>();

		_ = await dbContext.Players.AddAsync(new Player
		{
			PlayerCode = playerCode,
			BrandId = brandId,
			Username = "Player",
			DefaultWalletCurrencyCode = "USD",
			Password = Array.Empty<byte>(),
			Salt = Array.Empty<byte>(),
			WithdrawPassword = Array.Empty<byte>(),
			WithdrawSalt = Array.Empty<byte>()
		});

		_ = fakeMessageSender.RequestAsync(
			Arg.Is("tgs.signin.daily"),
			Arg.Any<ReadOnlyMemory<byte>>())
			.Returns(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var processor = ActivatorUtilities.CreateInstance<SignInProcessor>(serviceScope.ServiceProvider);

				return processor.HandleAsync(data).Result;
			});

		var actual = await sut.SignInAsync(promotionId, playerCode, date);

		Assert.IsNotNull(actual);
	}

	[TestMethod]
	public async Task Already_SignIn_Will_Return_ExistsData()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = new TestSignInEventApplication();

		var sut = new SignInClient(fakeMessageSender);

		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var date = DateOnly.FromDateTime(DateTime.Now);
		var playerCode = "tester";

		await using var serviceScope = web.Services.CreateAsyncScope();

		var dbContext = serviceScope.ServiceProvider.GetRequiredService<TgsContext>();

		_ = await dbContext.Players.AddAsync(new Player
		{
			PlayerCode = playerCode,
			BrandId = brandId,
			Username = "Player",
			DefaultWalletCurrencyCode = "USD",
			Password = Array.Empty<byte>(),
			Salt = Array.Empty<byte>(),
			WithdrawPassword = Array.Empty<byte>(),
			WithdrawSalt = Array.Empty<byte>()
		});

		var existsSignInEvent = new Tgs.SignInEvents.Entities.SignInEvent
		{
			Id = Guid.NewGuid(),
			BrandId = brandId,
			Date = date,
			PlayerCode = playerCode,
			Combo = 2,
			PromotionId = promotionId,
			CreatedOn = DateTime.Now
		};
		_ = await dbContext.SignInEvents.AddAsync(existsSignInEvent);

		_ = await dbContext.SaveChangesAsync();

		_ = fakeMessageSender.RequestAsync(
			Arg.Is("tgs.signin.daily"),
			Arg.Any<ReadOnlyMemory<byte>>())
			.Returns(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var processor = ActivatorUtilities.CreateInstance<SignInProcessor>(serviceScope.ServiceProvider);

				return processor.HandleAsync(data).Result;
			});

		var actual = await sut.SignInAsync(promotionId, playerCode, date);

		Assert.IsNotNull(actual);
		Assert.AreEqual(existsSignInEvent.Id, actual.Id);
		Assert.AreEqual(existsSignInEvent.Combo, actual.Combo);
	}

	[TestMethod]
	public async Task Continuous_Sign_In_Combo_Will_Increment_One()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = new TestSignInEventApplication();

		var sut = new SignInClient(fakeMessageSender);

		var promotionId = Guid.NewGuid();
		var brandId = Guid.NewGuid();
		var date = DateOnly.FromDateTime(DateTime.Now);
		var playerCode = "tester";

		await using var serviceScope = web.Services.CreateAsyncScope();

		var dbContext = serviceScope.ServiceProvider.GetRequiredService<TgsContext>();

		_ = await dbContext.Players.AddAsync(new Player
		{
			PlayerCode = playerCode,
			BrandId = brandId,
			Username = "Player",
			DefaultWalletCurrencyCode = "USD",
			Password = Array.Empty<byte>(),
			Salt = Array.Empty<byte>(),
			WithdrawPassword = Array.Empty<byte>(),
			WithdrawSalt = Array.Empty<byte>()
		});

		var existsSignInEvent = new Tgs.SignInEvents.Entities.SignInEvent
		{
			Id = Guid.NewGuid(),
			BrandId = brandId,
			Date = date.AddDays(-1),
			PlayerCode = playerCode,
			Combo = 2,
			PromotionId = promotionId,
			CreatedOn = DateTime.Now
		};
		_ = await dbContext.SignInEvents.AddAsync(existsSignInEvent);

		_ = await dbContext.SaveChangesAsync();

		_ = fakeMessageSender.RequestAsync(
			Arg.Is("tgs.signin.daily"),
			Arg.Any<ReadOnlyMemory<byte>>())
			.Returns(callInfo =>
			{
				var data = callInfo.Arg<ReadOnlyMemory<byte>>();

				var processor = ActivatorUtilities.CreateInstance<SignInProcessor>(serviceScope.ServiceProvider);

				return processor.HandleAsync(data).Result;
			});

		var actual = await sut.SignInAsync(promotionId, playerCode, date);

		Assert.IsNotNull(actual);
		Assert.AreNotEqual(existsSignInEvent.Id, actual.Id);
		Assert.AreEqual(existsSignInEvent.Combo + 1, actual.Combo);
	}
}
