namespace NShared.SetEquipSwap;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] EquippedSlotIndex { get; set; }

	public int[] UnEquippedSlotIndex { get; set; }
}
