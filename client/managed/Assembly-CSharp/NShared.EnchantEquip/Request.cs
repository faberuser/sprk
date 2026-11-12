namespace NShared.EnchantEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/enchant_equip";

	public int EquipItemSlotIndex { get; set; }

	public int EnchantItemIndex { get; set; }
}
