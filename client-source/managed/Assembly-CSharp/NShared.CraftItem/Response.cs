namespace NShared.CraftItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public CraftSlotResultInfo CraftSlotResult { get; set; }
}
