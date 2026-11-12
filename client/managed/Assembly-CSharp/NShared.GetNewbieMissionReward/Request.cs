namespace NShared.GetNewbieMissionReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "newbie_mission/get_newbie_mission_reward";

	public int MissionIndex { get; set; }
}
