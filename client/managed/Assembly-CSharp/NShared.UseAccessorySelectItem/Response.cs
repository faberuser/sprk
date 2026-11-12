namespace NShared.UseAccessorySelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public PlayerAccessoryCostumeInfo PlayerAccessoryCostumeInfo { get; set; }

	public HeroCostumeResultInfo HeroCostumeResultInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
