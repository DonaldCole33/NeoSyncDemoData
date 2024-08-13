using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RevogeneDemo.Resources;

public class DeviceResultRecordResource
{
	/// <summary>
	/// Sequence number for multiple result records
	/// </summary>
	/// <example>1</example>
	[Range(1, 1000)]
	public int SequenceNumber { get; set; }

	/// <summary>
	/// The name of the analyte
	/// </summary>
	/// <example>SARS</example>
	[Required(AllowEmptyStrings = false)]
	public string AnalyteName { get; set; }

	/// <summary>
	/// The result of the test. Possible values are numeric values, positive, negative and
	/// invalid for patient results, passed and failed for Calibration and QC results.
	/// </summary>
	/// <example>12.56</example>
	[Required(AllowEmptyStrings = false)]
	public string TestValue { get; set; }

	/// <summary>
	/// the units used to measure the result value
	/// </summary>
	/// <example>ug</example>
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string TestUnits { get; set; }

	/// <summary>
	/// Start of the valid range for the result. Empty for qualitative tests
	/// </summary>
	/// <example>11.00</example>
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public double? ReferenceRangeStart { get; set; }

	/// <summary>
	/// End of the valid range for the result. Empty for qualitative tests
	/// </summary>
	/// <example>15.00</example>
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public double? ReferenceRangeEnd { get; set; }

	[JsonIgnore]
	public string ReferenceRange
	{
		get
		{
			if (ReferenceRangeStart == null) return null;
			if (ReferenceRangeEnd == null) return ReferenceRangeStart.Value.ToString();
			return $"{ReferenceRangeStart}-{ReferenceRangeEnd}";
		}
	}

	/// <summary>
	/// The date that the test was completed. An ISO 8601 UTC Date is expected.
	/// </summary>
	/// <example>2022-02-25T04:08:49+0000</example>
	[Required]
	public DateTimeOffset TestDate { get; set; }
}