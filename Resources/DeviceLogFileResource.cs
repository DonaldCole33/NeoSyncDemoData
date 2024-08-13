using System.ComponentModel.DataAnnotations;

namespace RevogeneDemo.Resources;

public class DeviceLogFileResource : BaseDeviceResource
{
	/// <summary>
	/// Log name
	/// </summary>
	/// <example>test_log.txt</example>
	[Required(AllowEmptyStrings = false)]
	public string LogName { get; set; }

	/// <summary>
	/// File content in base64
	/// </summary>
	/// <example>ZXhhbXBsZQ==</example>
	[Required(AllowEmptyStrings = false)]
	public string Payload { get; set; }
}