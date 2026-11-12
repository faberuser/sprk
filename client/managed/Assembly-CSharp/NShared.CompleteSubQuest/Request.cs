namespace NShared.CompleteSubQuest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "quest/complete_sub_quest";

	public int[] SubQuestIndices { get; set; }
}
