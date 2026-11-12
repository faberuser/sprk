namespace NShared.GetEventStepInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EventStepInfo EventStepInfo { get; set; }

	public EventStepGroupServerInfo[] EventStepGroupServerInfos { get; set; }
}
