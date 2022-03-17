namespace Tgs.SignInEvents;

public record SignInRecord(Guid Id, string PlayerCode, DateOnly Date, int Combo);
