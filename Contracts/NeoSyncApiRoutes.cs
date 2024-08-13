namespace RevogeneDemo.Contracts;

public static class NeoSyncApiRoutes
{
	private const string BASE_ROUTE = "api/";
	public const string PING = BASE_ROUTE + "ping";

	public static class V1
	{
		private const string V1_ROUTE = BASE_ROUTE + "v1/";

		public static class Device
		{
			private const string DEVICE_ROUTE = V1_ROUTE + "device/";
			public const string REGISTER = DEVICE_ROUTE + "register/";

			public static class Configuration
			{
				private const string CONFIGURATION_ROUTE = DEVICE_ROUTE + "configuration/";
				public const string UPLOAD = CONFIGURATION_ROUTE + "upload";
			}

			public static class Logs
			{
				private const string LOG_ROUTE = DEVICE_ROUTE + "logs/";
				public const string UPLOAD = LOG_ROUTE + "upload";
			}

			public static class Images
			{
				private const string IMAGES_ROUTE = DEVICE_ROUTE + "images/";
				public const string REQUEST_UPLOAD = IMAGES_ROUTE + "request_upload";
			}

			public static class Firmware
			{
				private const string FIRMWARE_ROUTE = DEVICE_ROUTE + "firmware/";
				public const string CHECK_FOR_UPDATE = FIRMWARE_ROUTE + "check-for-update";
				public const string DOWNLOAD = FIRMWARE_ROUTE + "download/{id}";
			}

			public static class Metrics
			{
				private const string METRICS_ROUTE = DEVICE_ROUTE + "metrics/";
				public const string SEND = METRICS_ROUTE + "send";
			}

			public static class Results
			{
				public const string RESULTS_ROUTE = DEVICE_ROUTE + "results/";
			}
		}
	}
}