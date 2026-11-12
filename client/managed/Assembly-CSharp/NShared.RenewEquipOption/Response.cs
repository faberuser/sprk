namespace NShared.RenewEquipOption;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo ResultEquipItem { get; set; }

	public int NewOptionIndex { get; set; }

	public int NewOptionStep { get; set; }
}
