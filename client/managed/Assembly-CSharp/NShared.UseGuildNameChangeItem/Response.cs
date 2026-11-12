namespace NShared.UseGuildNameChangeItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }
}
