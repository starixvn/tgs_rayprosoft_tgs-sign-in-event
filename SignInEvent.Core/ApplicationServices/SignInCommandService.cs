using Tgs.ServiceUtility;

namespace Tgs.SignInEvents.ApplicationServices;

public class SignInCommandService : ICommandService<SignInCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public SignInCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(SignInCommand command, CancellationToken cancellationToken = default)
	{
		var currentRecord = await _tgsRepository.GetSignInRecordAsync(
			command.PromotionId,
			command.PlayerCode,
			command.Date,
			cancellationToken).ConfigureAwait(false);

		if (currentRecord is not null)
			return;

		var yesterday = command.Date.AddDays(-1);
		var yesterdayRecord = await _tgsRepository.GetSignInRecordAsync(
			command.PromotionId,
			command.PlayerCode,
			yesterday,
			cancellationToken).ConfigureAwait(false);

		var combo = yesterdayRecord is null
			? 1
			: yesterdayRecord.Combo + 1;

		await _tgsRepository.SignInAsync(
			command.PromotionId,
			command.PlayerCode,
			command.Date,
			combo,
			cancellationToken).ConfigureAwait(false);
	}
}
