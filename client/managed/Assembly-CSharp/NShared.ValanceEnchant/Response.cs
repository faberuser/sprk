namespace NShared.ValanceEnchant;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipItemInfo ResultEquipItem { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo ItemResultInfo { get; set; }

	public ValanceEnchantOptionInfo NewValanceEnchantOptionInfo { get; set; }
}
