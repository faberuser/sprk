namespace NShared.UnlockEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/unlock_equip";

	public int HeroIndex { get; set; }

	public int HeroPartIndex { get; set; }
}
