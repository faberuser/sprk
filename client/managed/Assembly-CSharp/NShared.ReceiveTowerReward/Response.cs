namespace NShared.ReceiveTowerReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public TowerInfo TowerInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }
}
