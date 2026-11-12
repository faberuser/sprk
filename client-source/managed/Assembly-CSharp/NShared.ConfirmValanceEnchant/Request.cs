namespace NShared.ConfirmValanceEnchant;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "valance/confirm_valance_enchant";

	public int EquipItemSlotIndex { get; set; }

	public bool IsNew { get; set; }
}
