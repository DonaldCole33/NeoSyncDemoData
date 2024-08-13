using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RevogeneDemo.Services;

await Host.CreateDefaultBuilder()
.ConfigureServices((context, collection) =>
{
	collection.AddHttpClient<NeoSyncCommunicationService>();
	collection.AddScoped<IDirectoryScanner, DirectoryScanner>();
	collection.AddHostedService<SchedulingService>();
})
.ConfigureLogging((context, builder) =>
	{
		builder.AddConsole();
		//builder.AddEventLog();
		builder.AddConfiguration(context.Configuration);
	})
.Build()
.RunAsync();