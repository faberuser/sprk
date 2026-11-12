namespace NShared.ValanceEnchant;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "valance/valance_enchant";

	public int EquipItemSlotIndex { get; set; }

	public int ConsumeItemIndex { get; set; }
}
