namespace NShared.ValanceCraft;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo EquipItemResult { get; set; }

	public int IsAdvanced { get; set; }
}
