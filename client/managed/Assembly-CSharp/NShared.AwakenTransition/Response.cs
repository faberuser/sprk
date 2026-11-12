namespace NShared.AwakenTransition;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public EquipItemInfo SourceEquipItemResult { get; set; }

	public EquipItemInfo TargetEquipItemResult { get; set; }
}
