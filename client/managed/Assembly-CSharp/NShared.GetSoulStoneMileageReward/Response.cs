namespace NShared.GetSoulStoneMileageReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public int SoulStoneMileage { get; set; }

	public ItemResultInfo[] RemovedItemResultInfos { get; set; }

	public int[] SelectSoulStoneIndices { get; set; }
}
