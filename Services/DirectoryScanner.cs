using System.Diagnostics.Metrics;
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RevogeneDemo.Contracts.Enums;
using RevogeneDemo.Resources;

namespace RevogeneDemo.Services;

public interface IDirectoryScanner
{
	Task Run();
}

public class DirectoryScanner : IDirectoryScanner
{
	private readonly ILogger<DirectoryScanner> _logger;
	private readonly NeoSyncCommunicationService _neoSyncCommunicationService;
	private readonly string _directoryToScan;

	// memory-only file persistence to avoid duplicating results
	private readonly List<string> _processedSrrFiles = new List<string>();
	private readonly List<string> _processedSerFiles = new List<string>();
	private readonly List<string> _processedSurFiles = new List<string>();

	private readonly string _serialNumber = "456def";

	public DirectoryScanner(ILogger<DirectoryScanner> logger, IConfiguration config, NeoSyncCommunicationService neoSyncCommunicationService)
	{
		_logger = logger;
		_neoSyncCommunicationService = neoSyncCommunicationService;
		_directoryToScan = config.GetValue<string>("DirectoryToScan")!;
	}

	public async Task Run()
	{
		try
		{
			await ScanDirectory();
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Failed during scan and process directory");
		}
	}

	private async Task ScanDirectory()
	{
		_logger.LogInformation($"Scanning {_directoryToScan}");
		if (!Directory.Exists(_directoryToScan))
		{
			_logger.LogError($"Directory {_directoryToScan} does not exist. Aborting scan.");
			return;
		}

		DateTime startTime = DateTime.Now;
		int resultsProcessed = 0;
		int eventsProcessed = 0;
		int metricsProcessed = 0;
		DirectoryInfo directoryToScan = new (_directoryToScan);

		// Get Sample Results Reports and push data to NeoSync
		var result = GetResultsFromHeader();
		await _neoSyncCommunicationService.SendResults(result);

		// Get System Events Reports and upload as logs to NeoSync
		foreach (var fileInfo in directoryToScan.GetFiles("SER_*.txt"))
		{
			if (_processedSerFiles.Contains(fileInfo.Name)) continue;
			await ProcessSerTxtFile(fileInfo);
			_processedSerFiles.Add(fileInfo.Name);
			eventsProcessed++;
		}

		// Get System Usage Report and upload metrics to NeoSync
		var metrics = GetMetricsFromHeader();
		await _neoSyncCommunicationService.SendMetrics(metrics);

		// todo: come up with some related metrics for the results? maybe from SUR files?
		DateTime endTime = DateTime.Now;
		_logger.LogInformation($"Scanning finished! start: {startTime:O} finish: {endTime:O}, duration: {endTime.Subtract(startTime).TotalSeconds:F3} secs");
		_logger.LogInformation($"Result reports processed: {resultsProcessed}, Event reports processed: {eventsProcessed}, Metric reports processed: {metricsProcessed}");
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

	private DeviceResultResource GetResultsFromHeader()
	{
		var result = new DeviceResultResource
		{
			Version = 1,
			MessageDate = DateTimeOffset.Now,
			SampleType = SampleType.QualityControl,
			SerialNumber = _serialNumber,
			DeviceId = _serialNumber,
			FirmwareVersion = "1.5.3",
			LocationName = "San Francisco Lab",
			CassetteTestType = "Sample 1",
			ResultRecords = new List<DeviceResultRecordResource>(),
			LotNumber = "Lot A" // required
		};
		var assayName = "Assay LDT";
		var assayVersion = "1.1";
		var assayClassification = "LDT";
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

	private DeviceMetricsResource GetMetricsFromHeader()
	{
		var now = DateTimeOffset.Now;
		var dateString = now.DateTime.ToString("MM/dd/yyyy HH:mm");
		var date = DateTimeOffset.ParseExact(dateString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US"));
		var metrics = new DeviceMetricsResource
		{
			SerialNumber = _serialNumber,
			Metrics = new List<DeviceMetricResource>
			{
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "System Uptime Hours",
					Value = 5
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Cumulative Testing Days",
					Value = 6
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Total Tests",
					Value = 15
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Passed Tests",
					Value = 15
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Failed Tests",
					Value = 0
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Success Rate",
					Value = 100
				},
				new DeviceMetricResource{
					Timestamp = date,
					Name = "Instrument Errors",
					Value = 1
				},
								new DeviceMetricResource{
					Timestamp = date,
					Name = "Widget 1 RPMs",
					Value = 2000
				},
												new DeviceMetricResource{
					Timestamp = date,
					Name = "Pipette 1000ml Fills",
					Value = 2346
				},
												new DeviceMetricResource{
					Timestamp = date,
					Name = "Feature X Activated # times",
					Value = 3
				}
			}
		};

		// foreach (XmlNode assayRecord in header.ParentNode!.SelectNodes("AssayRecords/AssayRecord")!)
		// {
		// 	if (assayRecord.SelectSingleNode("Lot_Number")?.InnerText != "--") continue;
		// 	var name = assayRecord.SelectSingleNode("Assay")!.InnerText.Trim();
		// 	metrics.Metrics.Add(new DeviceMetricResource
		// 	{
		// 		Timestamp = date,
		// 		Name = $"{name} Success Rate",
		// 		Value = double.Parse(header.SelectSingleNode("Success_Rate")!.InnerText.Replace("%", string.Empty))
		// 	});
		// }

		return metrics;
	}
}