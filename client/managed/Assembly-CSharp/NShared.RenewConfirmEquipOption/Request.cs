namespace NShared.RenewConfirmEquipOption;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/renew_confirm_equip_option";

	public int EquipItemSlotIndex { get; set; }

	public int ConfirmOptionIndex { get; set; }

	public int OptionSlotIndex { get; set; }

	public bool IsNew { get; set; }
}
