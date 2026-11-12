namespace NShared.GetCertificate;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_certificate";
}
