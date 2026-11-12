namespace NShared.SweepDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StaminaResultInfo StaminaResult { get; set; }

	public int StartDungeonIndex { get; set; }

	public int EndDungeonIndex { get; set; }

	public ChapterDungeonInfo[] CampaignResults { get; set; }

	public TowerInfo TowerInfoResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }
}
