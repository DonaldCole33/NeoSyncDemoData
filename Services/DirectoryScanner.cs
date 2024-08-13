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
		foreach (var fileInfo in directoryToScan.GetFiles("SRR_*.xml"))
		{
			if (_processedSrrFiles.Contains(fileInfo.Name)) continue;
			await ProcessSrrXmlFile(fileInfo);
			_processedSrrFiles.Add(fileInfo.Name);
			resultsProcessed++;
		}

		// Get System Events Reports and upload as logs to NeoSync
		foreach (var fileInfo in directoryToScan.GetFiles("SER_*.txt"))
		{
			if (_processedSerFiles.Contains(fileInfo.Name)) continue;
			await ProcessSerTxtFile(fileInfo);
			_processedSerFiles.Add(fileInfo.Name);
			eventsProcessed++;
		}

		// Get System Usage Report and upload metrics to NeoSync
		foreach (var fileInfo in directoryToScan.GetFiles("SUR_*.xml"))
		{
			if (_processedSurFiles.Contains(fileInfo.Name)) continue;
			await ProcessSurXmlFile(fileInfo);
			_processedSurFiles.Add(fileInfo.Name);
			metricsProcessed++;
		}

		// todo: come up with some related metrics for the results? maybe from SUR files?
		DateTime endTime = DateTime.Now;
		_logger.LogInformation($"Scanning finished! start: {startTime:O} finish: {endTime:O}, duration: {endTime.Subtract(startTime).TotalSeconds:F3} secs");
		_logger.LogInformation($"Result reports processed: {resultsProcessed}, Event reports processed: {eventsProcessed}, Metric reports processed: {metricsProcessed}");
	}

	private async Task ProcessSrrXmlFile(FileInfo fileInfo)
	{
		XmlDocument doc = new XmlDocument();
		doc.Load(fileInfo.FullName);
		var result = GetResultsFromHeader(doc.DocumentElement!.SelectSingleNode("Header")!);
		await _neoSyncCommunicationService.SendResults(result);
	}

	private async Task ProcessSurXmlFile(FileInfo fileInfo)
	{
		XmlDocument doc = new XmlDocument();
		doc.Load(fileInfo.FullName);
		var result = GetMetricsFromHeader(doc.DocumentElement!.SelectSingleNode("Header")!);
		await _neoSyncCommunicationService.SendMetrics(result);
	}

	private async Task ProcessSerTxtFile(FileInfo fileInfo)
	{
		var bytes = await File.ReadAllBytesAsync(fileInfo.FullName);
		var resource = new DeviceLogFileResource
		{
			LogName = fileInfo.Name,
			SerialNumber = fileInfo.Name.Split('_')[1],
			Payload = Convert.ToBase64String(bytes)
		};
		await _neoSyncCommunicationService.UploadLogs(resource);
	}

	private DeviceResultResource GetResultsFromHeader(XmlNode header)
	{
		var result = new DeviceResultResource
		{
			Version = 1,
			MessageDate = DateTimeOffset.Now,
			SampleType = SampleType.QualityControl,
			SerialNumber = header.SelectSingleNode("SerialNumber")!.InnerText,
			DeviceId = header.SelectSingleNode("SerialNumber")!.InnerText,
			FirmwareVersion = header.SelectSingleNode("SoftwareVersion")!.InnerText,
			LocationName = header.SelectSingleNode("LabName")!.InnerText,
			CassetteTestType = header.SelectSingleNode("SampleId")!.InnerText,
			ResultRecords = new List<DeviceResultRecordResource>(),
			LotNumber = "N/A" // required
		};
		var assayName = header.SelectSingleNode("AssayName")!.InnerText;
		var assayVersion = header.SelectSingleNode("AssayVersion")!.InnerText;
		var assayClassification = header.SelectSingleNode("AssayClassification")!.InnerText;
		var assay = $"{assayName} ({assayVersion} / {assayClassification})";
		var date = DateTimeOffset.Parse(header.SelectSingleNode("Started")!.InnerText);
		var sequence = 1;
		foreach (XmlNode targetResult in header.SelectNodes("TargetResults/TargetResult"))
		{
			result.ResultRecords.Add(new DeviceResultRecordResource
			{
				SequenceNumber = sequence++,
				AnalyteName = assay,
				TestDate = date,
				TestUnits = targetResult.SelectSingleNode("TargetName")!.InnerText,
				TestValue = targetResult.SelectSingleNode("Result")!.InnerText,
			});
		}
		return result;
	}

	private DeviceMetricsResource GetMetricsFromHeader(XmlNode header)
	{
		var dateString = header.SelectSingleNode("Created_Date")!.InnerText;
		var date = DateTimeOffset.ParseExact(dateString, "MM/dd/yyyy HH:mm", new DateTimeFormatInfo());
		var metrics = new DeviceMetricsResource
		{
			SerialNumber = header.SelectSingleNode("Instrument_Serial_Number")!.InnerText,
			Metrics = new List<DeviceMetricResource>
			{
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "System Uptime Hours",
					Value = double.Parse(header.SelectSingleNode("System_Uptime_Hours")!.InnerText)
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Cumulative Testing Days",
					Value = double.Parse(header.SelectSingleNode("Cumulative_Testing_Days")!.InnerText)
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Total Tests",
					Value = double.Parse(header.SelectSingleNode("Total_Tests")!.InnerText)
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Passed Tests",
					Value = double.Parse(header.SelectSingleNode("Complete")!.InnerText)
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Failed Tests",
					Value = double.Parse(header.SelectSingleNode("Failed")!.InnerText)
				},
				new DeviceMetricResource
				{
					Timestamp = date,
					Name = "Overall Success Rate",
					Value = double.Parse(header.SelectSingleNode("Success_Rate")!.InnerText.Replace("%", string.Empty))
				},
			}
		};

		foreach (XmlNode assayRecord in header.ParentNode!.SelectNodes("AssayRecords/AssayRecord")!)
		{
			if (assayRecord.SelectSingleNode("Lot_Number")?.InnerText != "--") continue;
			var name = assayRecord.SelectSingleNode("Assay")!.InnerText.Trim();
			metrics.Metrics.Add(new DeviceMetricResource
			{
				Timestamp = date,
				Name = $"{name} Success Rate",
				Value = double.Parse(header.SelectSingleNode("Success_Rate")!.InnerText.Replace("%", string.Empty))
			});
		}

		return metrics;
	}
}