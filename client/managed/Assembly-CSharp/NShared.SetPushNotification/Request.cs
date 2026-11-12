namespace NShared.SetPushNotification;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/set_push_notification";

	public PushNotificationType PushType { get; set; }

	public bool Status { get; set; }
}
