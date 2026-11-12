namespace NShared.SetChest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/set_chest";

	public int[] EquipItemSlotIndex { get; set; }
}
