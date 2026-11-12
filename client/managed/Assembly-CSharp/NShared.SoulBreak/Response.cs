namespace NShared.SoulBreak;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }
}
