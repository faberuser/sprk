namespace NShared.GetReplay;

[Response]
public class Response : GenericResponse<ResultType>
{
	public Replay Replay { get; set; }
}
