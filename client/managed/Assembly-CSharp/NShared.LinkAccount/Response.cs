namespace NShared.LinkAccount;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string SignOutDoneTime { get; set; }
}
