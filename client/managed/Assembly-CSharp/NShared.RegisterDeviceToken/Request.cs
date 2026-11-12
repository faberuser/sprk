namespace NShared.RegisterDeviceToken;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/register_device_token";

	public string DeviceToken { get; set; }
}
