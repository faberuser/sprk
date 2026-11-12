namespace NShared.ReceiveMazeTowerReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "maze_tower/receive_maze_tower_reward";

	public int MazeTowerRewardIndex { get; set; }
}
