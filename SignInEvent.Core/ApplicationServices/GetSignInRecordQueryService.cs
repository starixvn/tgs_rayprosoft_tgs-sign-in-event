using Tgs.ServiceUtility;

namespace Tgs.SignInEvents.ApplicationServices;

internal class GetSignInRecordQueryService : IGetService<GetSignInRecordQuery, SignInRecord?>
{
	private readonly ITgsRepository _tgsRepository;

	public GetSignInRecordQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask<SignInRecord?> GetAsync(GetSignInRecordQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.GetSignInRecordAsync(
			query.PromotionId,
			query.PlayerCode,
			query.Date,
			cancellationToken);
}
