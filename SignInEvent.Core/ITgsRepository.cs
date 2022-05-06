namespace Tgs.SignInEvents;

public interface ITgsRepository
{
	ValueTask<SignInRecord?> GetSignInRecordAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask SignInAsync(Guid promotionId, string playerCode, DateOnly date, int combo, CancellationToken cancellationToken = default);
}
