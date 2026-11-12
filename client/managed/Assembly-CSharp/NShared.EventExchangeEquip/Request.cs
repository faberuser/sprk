namespace NShared.EventExchangeEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event_equip/event_exchange_equip";

	public int[] EquipItemSlotIndices { get; set; }
}
