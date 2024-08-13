using System.ComponentModel.DataAnnotations;

namespace RevogeneDemo.Resources;

/// <summary>
/// Device metrics resource
/// </summary>
public class DeviceMetricsResource : BaseDeviceResource
{
	/// <summary>
	/// Device metrics
	/// </summary>
	[Required, MinLength(1)]
	public List<DeviceMetricResource> Metrics { get; set; }
}