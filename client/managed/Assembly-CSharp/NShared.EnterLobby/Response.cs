namespace NShared.EnterLobby;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string ForwardHost { get; set; }

	public string ServerLocalTime { get; set; }

	public string ServerUTCTime { get; set; }

	public int ServerUTCOffsetHour { get; set; }

	public ChapterDungeonInfo[] DungeonInfos { get; set; }

	public HideoutDungeonInfo[] HideoutDungeonInfos { get; set; }

	public ConquestDungeonInfo[] ConquestDungeonInfos { get; set; }

	public TowerInfo[] TowerInfos { get; set; }

	public AttendanceData[] AttendanceDatas { get; set; }

	public LobbyNotice[] LobbyNotices { get; set; }

	public int CurStamina { get; set; }

	public string StaminaRechargeTime { get; set; }

	public ContentsStatusInfo[] ContentsStatuses { get; set; }

	public ContentsValueInfo[] ContentsValues { get; set; }

	public CraftSlotResultInfo[] CraftSlotResults { get; set; }

	public string DevParam { get; set; }

	public int DayOfYear { get; set; }

	public int WeekOfYear { get; set; }

	public int MonthOfYear { get; set; }

	public int MonthFromMillennium { get; set; }

	public int RemainDaySecond { get; set; }

	public int RemainWeekSecond { get; set; }

	public int RemainMonthSecond { get; set; }

	public int DoyRandomSeed { get; set; }

	public int WoyRandomSeed { get; set; }

	public int EventCalendarVersion { get; set; }

	public EventDungeonPointResultInfo EventDungeonPointResult { get; set; }

	public int LoginDailyCount { get; set; }

	public GuildArenaSessionInfo[] GuildArenaSessionInfo { get; set; }

	public PushRewardInfo PushRewardInfo { get; set; }

	public MissionCategoryInfo[] OpendMissionCategories { get; set; }

	public WebEventInfo WebEvent { get; set; }

	public GuildSuppressSessionInfo GuildSuppressSessionInfo { get; set; }

	public NewPayShopProductInfo[] PayShopProductInfos { get; set; }

	public PlayerProductPurchaseInfo[] PlayerProductPurchaseInfos { get; set; }

	public TopClearDungeonInfo[] TopClearDungeonInfos { get; set; }
}
