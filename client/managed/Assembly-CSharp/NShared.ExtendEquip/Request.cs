namespace NShared.ExtendEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/extend_equip";

	public int ReqGem { get; set; }
}
