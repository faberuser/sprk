namespace NShared.QuerySession;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool IsLogon { get; set; }
}
