namespace NShared.EnchantEquip;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipItemInfo ResultEquipItem { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public int EquipHeroIndex { get; set; }

	public int NewEnchantOptionIndex { get; set; }

	public int NewEnchantOptionStep { get; set; }
}
