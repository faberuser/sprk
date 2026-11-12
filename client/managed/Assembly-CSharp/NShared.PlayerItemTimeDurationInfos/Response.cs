namespace NShared.PlayerItemTimeDurationInfos;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerItemTimeDurationInfo[] ItemTimeDurations { get; set; }
}
