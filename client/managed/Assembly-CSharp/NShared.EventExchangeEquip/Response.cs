namespace NShared.EventExchangeEquip;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public int[] RemovedEquipItemSlotIndices { get; set; }
}
