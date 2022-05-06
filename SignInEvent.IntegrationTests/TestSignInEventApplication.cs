using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using OpenTelemetry.Exporter;
using Tgs.SignInEvents.Entities;

namespace SignInEvent.IntegrationTests;

internal class TestSignInEventApplication : WebApplicationFactory<Program>
{
	private readonly Action<IWebHostBuilder>? _webHostConfigure;

	public TestSignInEventApplication(Action<IWebHostBuilder>? webHostConfigure = null)
	{
		_webHostConfigure = webHostConfigure;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		_ = builder.ConfigureAppConfiguration(builder => builder.Sources.Clear());

		_ = builder.ConfigureServices(services => services
			  .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
				  sp => new ConfigureNamedOptions<OtlpExporterOptions>(
					  Options.DefaultName,
					  exporterOptions => exporterOptions.TimeoutMilliseconds = 0))
			  .AddFakeNatsMessageQueue()
			  .Replace(ServiceDescriptor.Singleton(sp =>
			  {
				  var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
				  _ = dbContextOptionsBuilder
					  .UseInMemoryDatabase(Guid.NewGuid().ToString())
					  .ConfigureWarnings(x => x.Ignore(
						  InMemoryEventId.TransactionIgnoredWarning));

				  return new TgsContext(dbContextOptionsBuilder.Options);
			  })));

		_webHostConfigure?.Invoke(builder);
	}
}
