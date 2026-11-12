namespace NShared.UnsetChest;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] UnequippedSlotIndexInChest { get; set; }
}
