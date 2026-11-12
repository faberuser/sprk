namespace NShared.BreakEquip;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }
}
