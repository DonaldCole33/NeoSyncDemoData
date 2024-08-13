using System.ComponentModel.DataAnnotations;

namespace RevogeneDemo.Resources;

public class BaseDeviceResource
{
	/// <summary>
	/// Serial number
	/// </summary>
	/// <example>SN12345</example>
	[Required]
	public string SerialNumber { get; set; }
}