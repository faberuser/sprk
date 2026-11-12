namespace NShared.GetPhaseStepInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "phase_step/get_phase_step_info";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
