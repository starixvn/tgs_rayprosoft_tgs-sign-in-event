namespace Tgs.SignInEvents;

public interface ITgsRepository
{
	ValueTask<SignInRecord?> GetSignInRecordAsync(string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask SignInAsync(string playerCode, DateOnly date, int combo, CancellationToken cancellationToken = default);
}
