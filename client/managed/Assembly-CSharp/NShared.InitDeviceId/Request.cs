namespace NShared.InitDeviceId;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/init_device_id";

	public string DeviceId { get; set; }

	public string CountryCode { get; set; }
}
