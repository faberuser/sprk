namespace NShared.GetClearMissionReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "clear_mission/get_clear_mission_reward";

	public int MissionIndex { get; set; }
}
