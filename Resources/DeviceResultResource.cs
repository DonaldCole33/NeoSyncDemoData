using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RevogeneDemo.Contracts.Enums;

namespace RevogeneDemo.Resources;

/// <summary>
/// Reader results shared with NeoSync will follow this schema
/// </summary>
public class DeviceResultResource : BaseDeviceResource, IValidatableObject
{
	public const string NEOSYNC_JSON_RESULT = "NSJ";

	/// <summary>
	/// Always 'NSJ'
	/// </summary>
	public string MessageId => NEOSYNC_JSON_RESULT;

	/// <summary>
	/// The version of the schema being implemented 
	/// </summary>
	/// <example>1</example>
	[Required, Range(1,1)]
	public int Version { get; set; }

	/// <summary>
	/// The date that this message was generated. An ISO 8601 UTC Date is expected.
	/// </summary>
	/// <example>2022-02-25T04:08:49+0000</example>
	[Required(AllowEmptyStrings = false)]
	public DateTimeOffset MessageDate { get; set; }

	/// <summary>
	/// A vendor-supplied value which identifies the device which is integrated
	/// </summary>
	/// <example>abcreader</example>
	[Required(AllowEmptyStrings = false)]
	public string DeviceId { get; set; }

	/// <summary>
	/// The version of firmware on the reader at the time of the test
	/// </summary>
	/// <example>1.7.0-alpha</example>
	[Required(AllowEmptyStrings = false)]
	public string FirmwareVersion { get; set; }

	/// <summary>
	/// The cassette test type. May be a LOINC value, but any value uniquely describing the test type for this device can be used.
	/// </summary>
	/// <example>CB Cass</example>
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string CassetteTestType { get; set; }

	/// <summary>
	/// A enumeration value indicating whether the test is a patient test, a QC test or for
	/// calibration
	/// </summary>
	/// <example>Patient</example>
	[JsonConverter(typeof(StringEnumConverter))]
	public SampleType SampleType { get; set; }

	/// <summary>
	/// This is the lot number of the cassette. Required for QC and Calibration results.
	/// </summary>
	/// <example>QCL1234</example>
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string LotNumber { get; set; }

	/// <summary>
	/// A clinic identifiers
	/// </summary>
	/// <example>west-side clinic</example>
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string LocationName { get; set; }

	/// <summary>
	/// A collection of result records from the reader
	/// </summary>
	[Required]
	public List<DeviceResultRecordResource> ResultRecords { get; set; }

	/// <summary>
	/// RequiredIf is not robust enough to deal with int and string enum values, hence implementation here.
	/// </summary>
	public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (SampleType == SampleType.Calibration && String.IsNullOrEmpty(LotNumber))
		{
			yield return new ValidationResult($"{nameof(LotNumber)} is required if SampleType is {nameof(SampleType.Calibration)}");
		}

		if (SampleType == SampleType.QualityControl && String.IsNullOrEmpty(LotNumber))
		{
			yield return new ValidationResult($"{nameof(LotNumber)} is required if SampleType is {nameof(SampleType.QualityControl)}");
		}

		if (MessageDate == default)
		{
			yield return new ValidationResult($"{nameof(MessageDate)} is required");
		}

		foreach (DeviceResultRecordResource record in ResultRecords)
		{
			if (record.TestDate == default)
			{
				yield return new ValidationResult($"{nameof(record.TestDate)} is required");
			}
		}
	}
}