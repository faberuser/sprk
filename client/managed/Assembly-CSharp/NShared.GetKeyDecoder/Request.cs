namespace NShared.GetKeyDecoder;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/get_key_decoder";
}
