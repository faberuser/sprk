namespace NShared.RestoreSoulStone;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/restore_soul_stone";

	public int SoulStoneRestoreIndex { get; set; }
}
