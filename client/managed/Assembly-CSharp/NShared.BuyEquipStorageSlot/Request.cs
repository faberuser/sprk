namespace NShared.BuyEquipStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_storage_slot/buy_equip_storage_slot";
}
