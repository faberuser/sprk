namespace NShared.BreakItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] DecItemResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
