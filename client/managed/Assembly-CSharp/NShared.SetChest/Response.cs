namespace NShared.SetChest;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] EquippedSlotIndexInChest { get; set; }
}
