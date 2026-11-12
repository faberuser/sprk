namespace NShared.ConfirmEquipEnchantOption;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/confirm_equip_enchant_option";

	public int EquipItemSlotIndex { get; set; }

	public int ConfirmEnchantOptionIndex { get; set; }

	public bool IsNew { get; set; }
}
