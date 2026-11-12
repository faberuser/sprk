namespace NShared.GetEventStepReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event_step/get_event_step_reward";

	public EventStepType EventStepType { get; set; }
}
