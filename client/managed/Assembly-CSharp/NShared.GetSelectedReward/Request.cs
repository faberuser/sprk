namespace NShared.GetSelectedReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/get_selected_reward";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public string[] ItemCodes { get; set; }
}
