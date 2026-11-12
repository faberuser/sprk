namespace NShared.EquipGachaCeilingReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }
}
