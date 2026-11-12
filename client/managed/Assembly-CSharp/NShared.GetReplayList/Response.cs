namespace NShared.GetReplayList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public Replay[] Replays { get; set; }
}
