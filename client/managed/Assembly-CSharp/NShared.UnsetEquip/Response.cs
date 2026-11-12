namespace NShared.UnsetEquip;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] UnequippedSlotIndex { get; set; }
}
