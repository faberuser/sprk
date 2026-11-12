namespace NShared.ReceiveCompletedReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ExpResultInfo ExpResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public WorldMapEventTimeInfo[] WorldMapEventTimeInfos { get; set; }

	public WorldMapEventInfo[] WorldMapEventInfos { get; set; }

	public WorldMapEventInfo[] RemovedWorldMapEventInfos { get; set; }
}
