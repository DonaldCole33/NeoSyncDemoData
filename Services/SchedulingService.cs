using FluentScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RevogeneDemo.Services;

public class SchedulingService : IHostedService
{
	private readonly ILogger _logger;
	private readonly IDirectoryScanner _scanner;

	public SchedulingService(ILogger<SchedulingService> logger, IDirectoryScanner scanner)
	{
		_logger = logger;
		_scanner = scanner;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var registry = new Registry();
		ScheduleDirectoryScan(registry);

		JobManager.Initialize(registry);
		return Task.CompletedTask;
	}

	private void ScheduleDirectoryScan(Registry registry)
	{
		registry.Schedule(TriggerDirectoryScan).NonReentrant().ToRunNow().AndEvery(30).Seconds();
		_logger.LogInformation("Schedule directory scan to run now and every 30 seconds");
	}

	private void TriggerDirectoryScan()
	{
		_logger.LogInformation($"Triggered directory scan at {DateTime.Now}");
		_scanner.Run().GetAwaiter().GetResult();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		JobManager.Stop();
		return Task.CompletedTask;
	}
}