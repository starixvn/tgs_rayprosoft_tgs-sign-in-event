namespace Tgs.SignInEvents.ApplicationServices;

public record SignInCommand(string PlayerCode, DateOnly Date);
