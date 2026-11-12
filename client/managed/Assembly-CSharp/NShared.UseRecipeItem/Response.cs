namespace NShared.UseRecipeItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }
}
