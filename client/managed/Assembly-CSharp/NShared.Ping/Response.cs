namespace NShared.Ping;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string ForwardHost { get; set; }
}
