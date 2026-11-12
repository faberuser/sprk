namespace NShared.UpgradeEquipOption;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public EquipItemInfo ResultEquipItem { get; set; }

	public ItemResultInfo ItemResult { get; set; }
}
