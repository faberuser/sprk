namespace NShared.BreakRune;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/break_rune";

	public int[] ItemIndices { get; set; }

	public int[] Counts { get; set; }

	public int[] EquipItemSlotIndices { get; set; }
}
