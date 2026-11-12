namespace NShared.DumpUser;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string Output { get; set; }
}
