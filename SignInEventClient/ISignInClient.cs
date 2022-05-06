namespace Tgs.SignInEvents.Clients;

public interface ISignInClient
{
	ValueTask<SignInRecord> SignInAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);

	ValueTask<SignInRecord?> GetSignInRecordAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);
}
