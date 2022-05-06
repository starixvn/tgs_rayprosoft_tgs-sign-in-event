using Google.Protobuf;
using SignInEventService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using Tgs.SignInEvents;
using Tgs.SignInEvents.ApplicationServices;

namespace SignInEventService.MessageQueue;

public class SignInQueryProcessor : IMessageProcessor
{
	private readonly IGetService<GetSignInRecordQuery, SignInRecord?> _getSignInRecordService;

	public SignInQueryProcessor(
		IGetService<GetSignInRecordQuery, SignInRecord?> getSignInRecordService)
	{
		_getSignInRecordService = getSignInRecordService ?? throw new ArgumentNullException(nameof(getSignInRecordService));
	}

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = SignInRequest.Parser.ParseFrom(data.Span);

		var signRecord = await _getSignInRecordService.GetAsync(
			new GetSignInRecordQuery(
				request.PromotionId,
				request.PlayerCode,
				request.Date),
			cancellationToken).ConfigureAwait(false);

		return signRecord is null
			? null
			: new SignInResponse
			{
				Id = signRecord?.Id,
				Combo = signRecord?.Combo ?? -1
			}.ToByteArray();
	}
}
