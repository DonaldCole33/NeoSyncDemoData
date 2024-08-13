using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RevogeneDemo.Contracts;
using RevogeneDemo.Resources;

namespace RevogeneDemo.Services;

public class NeoSyncCommunicationService
{
	private const string NotApplicable = "N/A";
	private readonly HttpClient _client;
	private readonly List<string> _registeredDevices = new ();

	public NeoSyncCommunicationService(HttpClient client, IConfiguration config)
	{
		client.BaseAddress = new Uri(config.GetValue<string>("NeoSync:Url")!);
		client.DefaultRequestHeaders.Add(Constants.Constants.Security.VENDOR_KEY_HEADER, config.GetValue<string>("NeoSync:VendorKey"));
		client.DefaultRequestHeaders.Add(Constants.Constants.Security.SITE_CODE_HEADER, config.GetValue<string>("NeoSync:SiteCode"));
		_client = client;
	}

	public async Task SendMetrics(DeviceMetricsResource resource)
	{
		await PostAsync(NeoSyncApiRoutes.V1.Device.Metrics.SEND, resource);
	}

	public async Task SendResults(DeviceResultResource resource)
	{
		await PostAsync(NeoSyncApiRoutes.V1.Device.Results.RESULTS_ROUTE, resource);
	}

	public async Task UploadLogs(DeviceLogFileResource resource)
	{
		await PostAsync(NeoSyncApiRoutes.V1.Device.Logs.UPLOAD, resource);
	}

	private async Task PostAsync(string route, BaseDeviceResource resource)
	{
		await RegisterDevice(resource);
		var responseMessage = await _client.PostAsync(route, GetJsonContent(resource));
		if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
		{
			_client.DefaultRequestHeaders.Authorization = null;
			await RegisterDevice(resource);
			responseMessage = await _client.PostAsync(route, GetJsonContent(resource));
		}

		responseMessage.EnsureSuccessStatusCode();
	}

	private async Task RegisterDevice(BaseDeviceResource resource)
	{
		if (!_registeredDevices.Contains(resource.SerialNumber))
		{
			_client.DefaultRequestHeaders.Authorization = null;
		}

		if (_client.DefaultRequestHeaders.Authorization == null)
		{
			var registrationResult = await _client.PostAsync(NeoSyncApiRoutes.V1.Device.REGISTER, GetJsonContent(
				new DeviceInformationResource
				{
					SerialNumber = resource.SerialNumber,
					FirmwareVersion = NotApplicable,
					HardwareRevision = NotApplicable,
					SoftwareVersion = NotApplicable
				}));
			registrationResult.EnsureSuccessStatusCode();
			var content = await registrationResult.Content.ReadAsStringAsync();
			var deviceRegistrationResponse = JsonConvert.DeserializeObject<DeviceRegistrationResponse>(content);
			_client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", deviceRegistrationResponse.AuthToken);
			_registeredDevices.Add(resource.SerialNumber);
		}
	}

	private static StringContent GetJsonContent(BaseDeviceResource resource)
	{
		return new StringContent(JsonConvert.SerializeObject(resource), Encoding.UTF8, "application/json");
	}
}