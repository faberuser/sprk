namespace NShared.RenewEquipOption;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/renew_equip_option";

	public int EquipItemSlotIndex { get; set; }

	public int OptionSlotIndex { get; set; }

	public int CouponItemIndex { get; set; }
}
