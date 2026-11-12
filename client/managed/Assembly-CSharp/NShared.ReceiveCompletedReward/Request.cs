namespace NShared.ReceiveCompletedReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "world_map/receive_completed_reward";

	public int ChapterIndex { get; set; }

	public int[] DungeonIndices { get; set; }

	public int[] EventIndices { get; set; }
}
