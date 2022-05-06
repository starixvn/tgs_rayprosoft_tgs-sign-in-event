namespace Tgs.SignInEvents.ApplicationServices;

public record SignInCommand(Guid PromotionId, string PlayerCode, DateOnly Date);
