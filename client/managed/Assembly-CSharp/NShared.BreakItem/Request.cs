namespace NShared.BreakItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/break_item";

	public int[] ItemIndices { get; set; }

	public int[] Counts { get; set; }
}
