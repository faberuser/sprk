namespace NShared.UnsetChest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/unset_chest";

	public int[] EquipItemSlotIndex { get; set; }
}
