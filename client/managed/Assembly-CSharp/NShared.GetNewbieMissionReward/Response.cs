namespace NShared.GetNewbieMissionReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public NewbieMissionInfo NewbieMissionResult { get; set; }
}
