namespace NShared.UpgradeEquip;

[Response]
public class Response : GenericResponse<ResultType>, IResponse
{
	public EquipItemInfo ResultEquipItem { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public int[] RemovedEquipItemSlotIndices { get; set; }
}
