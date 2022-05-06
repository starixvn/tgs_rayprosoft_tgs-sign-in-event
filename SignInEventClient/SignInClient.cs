using Google.Protobuf;
using SignInEventService.Protos;
using Tgs.MessageQueue;

namespace Tgs.SignInEvents.Clients;

internal class SignInClient : ISignInClient
{
	private readonly IMessageSender _messageSender;

	public SignInClient(IMessageSender messageSender)
	{
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	public async ValueTask<SignInRecord> SignInAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var request = new SignInRequest
		{
			PromotionId = promotionId,
			PlayerCode = playerCode,
			Date = date
		};

		var responseData = await _messageSender!
			.RequestAsync("tgs.signin.daily", request.ToByteArray(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var response = SignInResponse.Parser.ParseFrom(responseData.Span);

		return new SignInRecord(response.Id, response.Combo);
	}

	public async ValueTask<SignInRecord?> GetSignInRecordAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var request = new SignInRequest
		{
			PromotionId = promotionId,
			PlayerCode = playerCode,
			Date = date
		};

		var responseData = await _messageSender!
			.RequestAsync("tgs.signin.daily.query", request.ToByteArray(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var response = responseData.Length > 0
			? SignInResponse.Parser.ParseFrom(responseData.Span)
			: null;

		return response is null
			? null
			: new SignInRecord(response.Id, response.Combo);
	}
}
