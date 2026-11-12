namespace NShared.EventForge;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool Success { get; set; }

	public EquipItemInfo ResultEquipItem { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public int[] RemovedEquipItemSlotIndices { get; set; }

	public ItemResultInfo[] RemovedItemResultInfos { get; set; }
}
