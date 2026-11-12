namespace NShared.SoulBreak;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_break";

	public int[] EquipItemSlotIndices { get; set; }
}
