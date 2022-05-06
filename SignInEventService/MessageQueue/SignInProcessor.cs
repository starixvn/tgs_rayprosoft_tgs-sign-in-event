using Google.Protobuf;
using SignInEventService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using Tgs.SignInEvents;
using Tgs.SignInEvents.ApplicationServices;

namespace SignInEventService.MessageQueue;

public class SignInProcessor : IMessageProcessor
{
	private readonly ICommandService<SignInCommand> _signInService;
	private readonly IGetService<GetSignInRecordQuery, SignInRecord?> _getSignInRecordService;

	public SignInProcessor(
		ICommandService<SignInCommand> signInService,
		IGetService<GetSignInRecordQuery, SignInRecord?> getSignInRecordService)
	{
		_signInService = signInService ?? throw new ArgumentNullException(nameof(signInService));
		_getSignInRecordService = getSignInRecordService ?? throw new ArgumentNullException(nameof(getSignInRecordService));
	}

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = SignInRequest.Parser.ParseFrom(data.Span);

		var command = new SignInCommand(
			request.PromotionId,
			request.PlayerCode,
			request.Date);

		await _signInService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);

		var signRecord = await _getSignInRecordService.GetAsync(
			new GetSignInRecordQuery(
				request.PromotionId,
				request.PlayerCode,
				command.Date),
			cancellationToken).ConfigureAwait(false);

		return new SignInResponse
		{
			Id = signRecord!.Id,
			Combo = signRecord!.Combo
		}.ToByteArray();
	}
}
