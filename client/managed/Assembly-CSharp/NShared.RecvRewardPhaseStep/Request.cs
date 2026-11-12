namespace NShared.RecvRewardPhaseStep;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "phase_step/recv_reward_phase_step";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public string Type { get; set; }
}
