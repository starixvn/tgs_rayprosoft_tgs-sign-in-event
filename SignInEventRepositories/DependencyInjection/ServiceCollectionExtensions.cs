using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tgs.SignInEvents.Entities;

namespace Tgs.SignInEvents.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSignInEventRepositories(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> dbConfigure)
		=> services
			.AddDbContext<TgsContext>(dbConfigure)
			.AddTransient<ITgsRepository, TgsRepository>()
			.AddSerialNumberGenerator();
}
