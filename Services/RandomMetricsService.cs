using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NeoSyncDemoData.Contracts.Enums;
using NeoSyncDemoData.Resources;

namespace NeoSyncDemoData.Services;

public interface IRandomMetricsService
{
	Task Run();
}


public class RandomMetricsService : IRandomMetricsService
{
    private readonly ILogger<RandomMetricsService> _logger;
	private readonly NeoSyncCommunicationService _neoSyncCommunicationService;

    // memory-only file persistence to avoid duplicating results
    private readonly List<string> _instruments = new List<string>()
	{
        "Instrument1",
        "Instrument2",
        "Instrument3",
        "Instrument4",
        "Instrument5"
    };

	private readonly List<string> _processedSurFiles = new List<string>();

	private readonly string _serialNumber = "456def";

	public RandomMetricsService(ILogger<RandomMetricsService> logger, IConfiguration config, NeoSyncCommunicationService neoSyncCommunicationService)
	{
		_logger = logger;
		_neoSyncCommunicationService = neoSyncCommunicationService;
    }

    public async Task Run()
    {
        try
        {
            await UpdateMetrics();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed during update metrics");
        }
    }

    private DeviceMetricResource CreateMetric(string Type, double Value)
	{
		return new DeviceMetricResource()
        {
            Name = Type,
            Value = Value,
            Timestamp = DateTimeOffset.Now
        };
    }

	private async Task UpdateMetrics()
	{
		_logger.LogInformation($"Updating All Metrics");

		DateTime startTime = DateTime.Now;
		int resultsProcessed = 0;
		int eventsProcessed = 0;
		int metricsProcessed = 0;
		DeviceMetricsResource metrics;

		//update 5 devices
		//all metrics for each device, some randomized values

		foreach (var instrument in _instruments)
		{
			metrics = new DeviceMetricsResource
			{
				SerialNumber = instrument,
				Metrics = new List<DeviceMetricResource>
				{
					CreateMetric("SystemUptimeHours", 5),
					CreateMetric("CumulativeTestingDays", 6),
					CreateMetric("OverallTotalTests", 15),
					CreateMetric("OverallPassedTests", 15),
					CreateMetric("OverallFailedTests", 0),
					CreateMetric("OverallSuccess", 100),
					CreateMetric("InstrumentError", 1),
					CreateMetric("Widget1RPMs", 2000),
					CreateMetric("Pipette1000mlFills", 2346),
					CreateMetric("FeatureXActivatedNumTimes", 3),
					CreateMetric("SystemEventAOccured", 2)
				}
			};

			await _neoSyncCommunicationService.SendMetrics(metrics);
			metricsProcessed++;

			//Results for each sample type
			foreach (var sampleType in Enum.GetValues(typeof(SampleType)).Cast<SampleType>())
			{
				var result = GetDeviceResults(sampleType, instrument);
				await _neoSyncCommunicationService.SendResults(result);
				resultsProcessed++;
			}

			// Get System Events Reports and upload as logs to NeoSync
			//foreach (var fileInfo in directoryToScan.GetFiles("SER_*.txt"))
			//{
			//	if (_processedSerFiles.Contains(fileInfo.Name)) continue;
			//	await ProcessSerTxtFile(fileInfo);
			//	_processedSerFiles.Add(fileInfo.Name);
			//	eventsProcessed++;
			//}
		}

		
		DateTime endTime = DateTime.Now;
		_logger.LogInformation($"Update Metrics finished! start: {startTime:O} finish: {endTime:O}, duration: {endTime.Subtract(startTime).TotalSeconds:F3} secs");
		_logger.LogInformation($"Results processed: {resultsProcessed}, Events processed: {eventsProcessed}, Metrics processed: {metricsProcessed}");
	}

	private async Task ProcessSerTxtFile(FileInfo fileInfo)
	{
		var bytes = await File.ReadAllBytesAsync(fileInfo.FullName);
		var resource = new DeviceLogFileResource
		{
			LogName = fileInfo.Name,
			SerialNumber = _serialNumber,
			Payload = Convert.ToBase64String(bytes)
		};
		await _neoSyncCommunicationService.UploadLogs(resource);
	}

	private DeviceResultResource GetDeviceResults(SampleType sampleType, string serialNumber)
	{
		var result = new DeviceResultResource
		{
			Version = 1,
			MessageDate = DateTimeOffset.Now,
			SampleType = sampleType,
			SerialNumber = serialNumber,
			DeviceId = serialNumber,
			FirmwareVersion = "1.5.3",
			LocationName = "San Francisco Lab",
			CassetteTestType = "Sample 1",
			ResultRecords = new List<DeviceResultRecordResource>(),
			LotNumber = "Lot A" // required
		};
		
		var assayName = "TriVerity";
		var assayVersion = "1.1";
		var assayClassification = "IVD";
		var assay = $"{assayName} ({assayVersion} / {assayClassification})";
		var date = DateTimeOffset.Now.AddHours(-1);
		var sequence = 1;
		for (var i = 0; i < 5; i++) 
		{
			result.ResultRecords.Add(new DeviceResultRecordResource
			{
				SequenceNumber = sequence++,
				AnalyteName = assay,
				TestDate = date,
				TestUnits = $"Target {i+1}",
				TestValue = "Negative",
			});
		}
		return result;
	}

}