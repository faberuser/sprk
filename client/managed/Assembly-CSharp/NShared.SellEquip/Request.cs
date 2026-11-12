namespace NShared.SellEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/sell_equip";

	public int[] EquipItemSlotIndices { get; set; }
}
