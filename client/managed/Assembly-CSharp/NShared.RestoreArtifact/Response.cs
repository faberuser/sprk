namespace NShared.RestoreArtifact;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public EquipItemInfo EquipItem { get; set; }
}
