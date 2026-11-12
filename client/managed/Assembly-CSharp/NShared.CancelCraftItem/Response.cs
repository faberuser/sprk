namespace NShared.CancelCraftItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public CraftSlotResultInfo CraftSlotResult { get; set; }
}
