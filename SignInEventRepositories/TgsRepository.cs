using Microsoft.EntityFrameworkCore;
using Tgs.SerialNumberGenerator;
using Tgs.SignInEvents.Entities;

namespace Tgs.SignInEvents;

internal class TgsRepository : ITgsRepository
{
	private readonly TgsContext _tgsContext;
	private readonly ISerialNumberGenerator _serialNumberGenerator;

	public TgsRepository(TgsContext tgsContext, ISerialNumberGenerator serialNumberGenerator)
	{
		_tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
	}

	public async ValueTask<SignInRecord?> GetSignInRecordAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var record = await _tgsContext.SignInEvents
			.Where(s => s.PromotionId == promotionId && s.PlayerCode == playerCode && s.Date == date)
			.SingleOrDefaultAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return record is null
			? null
			: new SignInRecord(record.Id, record.PlayerCode, record.Date, record.Combo);
	}

	public async ValueTask SignInAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		int combo,
		CancellationToken cancellationToken = default)
	{
		var player = await _tgsContext.Players
			.FindAsync(new object?[] { playerCode }, cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		if (player == null)
			return;

		var id = await _serialNumberGenerator
			.GenerateAsync()
			.ConfigureAwait(false);

		var insertRecord = new SignInEvent
		{
			Id = id,
			PlayerCode = playerCode,
			BrandId = player.BrandId,
			PromotionId = promotionId,
			Date = date,
			Combo = combo
		};

		_ = await _tgsContext.SignInEvents
			.AddAsync(insertRecord, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext
			.SaveChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}
}
