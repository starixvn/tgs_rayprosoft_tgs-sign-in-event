namespace Tgs.SignInEvents.ApplicationServices;

public record GetSignInRecordQuery(Guid PromotionId, string PlayerCode, DateOnly Date);
