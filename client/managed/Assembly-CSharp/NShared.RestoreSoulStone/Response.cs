namespace NShared.RestoreSoulStone;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] RemovedItemResultInfos { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public int SoulStoneMileage { get; set; }

	public int[] SelectSoulStoneIndices { get; set; }
}
