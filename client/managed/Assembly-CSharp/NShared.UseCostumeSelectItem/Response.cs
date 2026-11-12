namespace NShared.UseCostumeSelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public HeroCostumeResultInfo HeroCostumeResult { get; set; }

	public CostumeInfo[] CostumeResults { get; set; }
}
