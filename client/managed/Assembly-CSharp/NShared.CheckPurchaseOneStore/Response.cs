namespace NShared.CheckPurchaseOneStore;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public PlayerPurchaseTimeDurationInfo purchaseTimeDuration { get; set; }

	public PlayerFirstPurchaseInfo firstPurchase { get; set; }

	public PlayerProductPurchaseInfo PlayerProductPurchaseInfo { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public HeroAddResultInfo[] HeroAddResults { get; set; }

	public AttendanceInfo Attendance { get; set; }

	public AttendanceData[] AttendanceDatas { get; set; }

	public int[] AttendanceRewardableDays { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }

	public PurchaseMarketingResultInfo PurchaseMarketingResult { get; set; }

	public NewbieMissionInfo[] NewbieMissionInfos { get; set; }

	public PlayerAnyMisc[] PlayerAnyMiscs { get; set; }

	public MissionCategoryInfo[] OpendMissionCategories { get; set; }
}
