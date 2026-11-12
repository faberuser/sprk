namespace NShared.EventForge;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event_equip/event_forge";

	public int EquipItemSlotIndex { get; set; }

	public int UpgradeLevel { get; set; }

	public bool IsFixed { get; set; }
}
