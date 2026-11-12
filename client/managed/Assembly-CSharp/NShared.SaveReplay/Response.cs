namespace NShared.SaveReplay;

[Response]
public class Response : GenericResponse<ResultType>
{
	public long ReplayUid { get; set; }
}
