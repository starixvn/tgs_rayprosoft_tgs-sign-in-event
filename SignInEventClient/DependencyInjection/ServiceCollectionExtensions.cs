using Tgs.SignInEvents.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSignInClient(this IServiceCollection services)
		=> services.AddTransient<ISignInClient, SignInClient>();
}
