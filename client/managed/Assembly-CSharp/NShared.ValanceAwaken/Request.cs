namespace NShared.ValanceAwaken;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "valance/valance_awaken";

	public int EquipItemSlotIndex { get; set; }
}
