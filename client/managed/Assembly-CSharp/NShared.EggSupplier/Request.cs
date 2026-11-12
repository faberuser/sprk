namespace NShared.EggSupplier;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/egg_supplier";
}
