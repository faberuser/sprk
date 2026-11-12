namespace NShared.GetAwakeMaterialChallenge;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }
}
