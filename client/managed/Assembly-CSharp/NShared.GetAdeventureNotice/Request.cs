namespace NShared.GetAdeventureNotice;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_adeventure_notice";
}
