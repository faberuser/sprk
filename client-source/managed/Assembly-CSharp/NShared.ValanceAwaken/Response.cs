namespace NShared.ValanceAwaken;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool Success { get; set; }

	public EquipItemInfo ResultEquipItem { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
