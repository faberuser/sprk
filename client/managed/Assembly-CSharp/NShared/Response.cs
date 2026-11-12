namespace NShared;

public class Response
{
	public BaseResultType BaseResult { get; set; }

	public string InternalErrorMessage { get; set; }

	public AchievementInfo[] AchievementInfos { get; set; }

	public ScenarioQuestInfo ReservedScenarioQuestInfo { get; set; }

	public StaminaResultInfo[] ReservedStaminaResults { get; set; }

	public PlayRecordInfo[] ReservedPlayRecordInfos { get; set; }

	public PlayerArchiveInfo[] ReservedPlayerArchiveInfos { get; set; }

	public MainQuestInfo ReservedMainQuestInfo { get; set; }

	public SubQuestInfo[] ReservedSubQuestInfos { get; set; }

	public ClearMissionInfo[] ReservedClearMissionInfos { get; set; }

	public UpdatedEquipItemInfo[] ReservedUpdatedEquipItemInfos { get; set; }

	public PurchaseMarketingInfo[] ReservedPurchaseMarketingInfos { get; set; }

	public PlayerKingsPassExp ReservePlayerKingsPassExpInfo { get; set; }

	public KingsPassMission[] ReservedKingsPassMissionInfos { get; set; }

	public CurrencyResultInfo3[] ReservedCurrencyResults { get; set; }

	public Notice[] ReservedNoticeInfos { get; set; }

	public TeamLevelBuff[] ReserveTeamLevelBuffInfos { get; set; }
}
