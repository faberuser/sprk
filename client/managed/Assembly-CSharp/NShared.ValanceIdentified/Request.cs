namespace NShared.ValanceIdentified;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "valance/valance_identified";

	public int[] EquipItemSlotIndices { get; set; }
}
