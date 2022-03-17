using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.SignInEvents;
using Tgs.SignInEvents.ApplicationServices;

namespace SignInEvent.UnitTests;

[TestClass]
public class SignInTests
{
	[TestMethod]
	public async Task Player_SignIn()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new SignInCommandService(fakeTgsRepository);

		var command = new SignInCommand("player1", DateOnly.FromDateTime(DateTime.Now));

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository.Received(1)
			.SignInAsync(
				Arg.Is(command.PlayerCode),
				Arg.Is(command.Date),
				Arg.Is(1),
				Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task Player_Combo_SignInAsync()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new SignInCommandService(fakeTgsRepository);

		var command = new SignInCommand("player1", DateOnly.FromDateTime(DateTime.Now));

		_ = fakeTgsRepository.GetSignInRecordAsync(
			Arg.Is(command.PlayerCode),
			Arg.Is(command.Date.AddDays(-1)),
			Arg.Any<CancellationToken>())
			.Returns(new SignInRecord(
				Guid.NewGuid(),
				command.PlayerCode,
				command.Date.AddDays(-1),
				2));

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository.Received(1)
			.SignInAsync(
				Arg.Is(command.PlayerCode),
				Arg.Is(command.Date),
				Arg.Is(3),
				Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task If_Already_SignIn_Should_Not_SignIn_Again()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new SignInCommandService(fakeTgsRepository);

		var command = new SignInCommand("player1", DateOnly.FromDateTime(DateTime.Now));

		_ = fakeTgsRepository.GetSignInRecordAsync(
			Arg.Is(command.PlayerCode),
			Arg.Is(command.Date),
			Arg.Any<CancellationToken>())
			.Returns(new SignInRecord(
				Guid.NewGuid(),
				command.PlayerCode,
				command.Date,
				2));

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository.Received(0)
			.SignInAsync(
				Arg.Any<string>(),
				Arg.Any<DateOnly>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>());
	}

	[TestMethod]
	public async Task Get_Player_SignInRecord()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new GetSignInRecordQueryService(fakeTgsRepository);

		var query = new GetSignInRecordQuery("player1", DateOnly.FromDateTime(DateTime.Now));

		var signInRecord = new SignInRecord(Guid.NewGuid(), query.PlayerCode, query.Date, 1);
		_ = fakeTgsRepository.GetSignInRecordAsync(
			Arg.Is(query.PlayerCode),
			Arg.Is(query.Date),
			Arg.Any<CancellationToken>())
			.Returns(signInRecord);

		var actual = await sut.GetAsync(query);

		Assert.AreEqual(signInRecord, actual);
	}

	[TestMethod]
	public async Task Player_Without_SignIn_Get_Record_Should_Return_Nil()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new GetSignInRecordQueryService(fakeTgsRepository);

		var query = new GetSignInRecordQuery("player1", DateOnly.FromDateTime(DateTime.Now));

		var actual = await sut.GetAsync(query);

		Assert.IsNull(actual);
	}
}
