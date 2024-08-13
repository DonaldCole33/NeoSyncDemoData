using System.ComponentModel.DataAnnotations;

namespace RevogeneDemo.Resources;

/// <summary>
/// Device metric resource
/// </summary>
public class DeviceMetricResource
{
	/// <summary>
	/// Metric name
	/// </summary>
	/// <example>batterLevel</example>>
	[Required, MaxLength(255)]
	public string Name { get; set; }

	/// <summary>
	/// Metric value
	/// </summary>
	/// <example>12.54</example>>
	[Required]
	public double Value { get; set; }

	/// <summary>
	/// The metric timestamp. An ISO 8601 UTC Date is expected.
	/// </summary>
	/// <example>2022-03-12T16:02:12+0000</example>
	[Required]
	public DateTimeOffset Timestamp { get; set; }
}