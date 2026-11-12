namespace NShared.AddCraftSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public CraftSlotResultInfo CraftSlotResult { get; set; }
}
