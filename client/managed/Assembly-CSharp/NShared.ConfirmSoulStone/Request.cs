namespace NShared.ConfirmSoulStone;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/confirm_soul_stone";

	public int SoulStoneItemIndex { get; set; }
}
