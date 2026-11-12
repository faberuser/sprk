namespace NShared.BreakRune;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] DecItemResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public int[] RemoveEquipItemSlotIndices { get; set; }
}
