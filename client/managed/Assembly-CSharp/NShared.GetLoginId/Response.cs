namespace NShared.GetLoginId;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string LoginId { get; set; }

	public string Message { get; set; }
}
