namespace RevogeneDemo.Contracts.Enums;

/// <summary>
/// An enumeration value indicating whether the test is a patient test, a QC test or for
/// calibration
/// </summary>
public enum SampleType
{
	Patient = 1,
	QualityControl = 2,
	Calibration = 3
}