namespace NShared.ExtendChest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/extend_chest";

	public int ReqGem { get; set; }
}
