using System.ComponentModel.DataAnnotations;

namespace NeoSyncDemoData.Resources;

/// <summary>
/// Device information resource
/// </summary>
public class BaseDeviceInformationResource
{
    /// <summary>
    /// Serial number
    /// </summary>
    /// <example>SN12345</example>
    [Required]
    public string SerialNumber { get; set; }

    /// <summary>
    /// Firmware version
    /// </summary>
    /// <example>13.25.14</example>
    [Required]
	public string FirmwareVersion { get; set; }

	/// <summary>
	/// Software version
	/// </summary>
	/// <example>3.0.12</example>
	[Required]
	public string SoftwareVersion { get; set; }

	/// <summary>
	/// Hardware Revision
	/// </summary>
	/// <example>v12.4</example>
	[Required]
	public string HardwareRevision { get; set; }
}