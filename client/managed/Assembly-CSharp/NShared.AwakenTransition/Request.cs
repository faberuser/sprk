namespace NShared.AwakenTransition;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/awaken_transition";

	public int ItemIndex { get; set; }

	public int SourceEquipItemSlotIndex { get; set; }

	public int TargetEquipItemSlotIndex { get; set; }
}
