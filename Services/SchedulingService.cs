using FluentScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NeoSyncDemoData.Services;

public class SchedulingService : IHostedService
{
	private readonly ILogger _logger;
	private readonly IRandomMetricsService _randomMetricsService;

	public SchedulingService(ILogger<SchedulingService> logger, IRandomMetricsService metricsService)
	{
		_logger = logger;
		_randomMetricsService = metricsService;
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
		_logger.LogInformation("Update Metrics to run now and every 30 seconds");
	}

	private void TriggerDirectoryScan()
	{
		_logger.LogInformation($"Triggered update metrics at {DateTime.Now}");
		_randomMetricsService.Run().GetAwaiter().GetResult();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		JobManager.Stop();
		return Task.CompletedTask;
	}
}