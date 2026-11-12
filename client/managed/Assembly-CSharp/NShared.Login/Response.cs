namespace NShared.Login;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string ServerPrivateIp { get; set; }

	public int ServerPort { get; set; }

	public int AdminLevel { get; set; }

	public string StickyHost { get; set; }

	public string AlternativeHost { get; set; }

	public bool IsNewUser { get; set; }

	public string ServerLocalTime { get; set; }

	public string ServerUTCTime { get; set; }

	public int ServerUTCOffsetHour { get; set; }

	public string LoginId { get; set; }

	public string FaceBookId { get; set; }

	public string GooglePlusId { get; set; }

	public string GameCenterId { get; set; }

	public string AppleId { get; set; }

	public LoginIdByMethodType[] LoginIdByMethodType { get; set; }

	public bool EventPush { get; set; }

	public bool GuildRaidPush { get; set; }

	public bool GuildSuppressPush { get; set; }

	public bool NightPush { get; set; }

	public UserInfo UserInfo { get; set; }

	public PlayerBattleInfo BattleInfo { get; set; }

	public PlayerBattleBanPickInfo BattleBanPickInfo { get; set; }

	public PlayerBattleLuckyInfo BattleLuckyInfo { get; set; }

	public PlayerMiscInfo MiscInfo { get; set; }

	public long GuildId { get; set; }

	public HeroInfo[] Heroes { get; set; }

	public HeroRunePageInfo[] HeroRunePageInfos { get; set; }

	public ItemInfo[] Items { get; set; }

	public EquipItemInfo[] EquipItems { get; set; }

	public SoulWeaponInfo[] SoulWeaponInfos { get; set; }

	public ChapterDungeonInfo[] ChapterDungeons { get; set; }

	public TowerInfo[] Towers { get; set; }

	public ShopRestockTimeInfo[] ShopRestockTimes { get; set; }

	public ShopListItemInfo[] ShopListItems { get; set; }

	public MessageServerInfo MessageServerInfo { get; set; }

	public FriendInfo[] FriendInfos { get; set; }

	public FriendUserInfo[] FriendInvitorInfos { get; set; }

	public TutorialInfo[] Tutorials { get; set; }

	public PlayerItemTimeDurationInfo[] ItemTimeDurations { get; set; }

	public PlayerFirstPurchaseInfo[] FirstPurchases { get; set; }

	public PlayerPurchaseTimeDurationInfo[] PurchaseTimeDurations { get; set; }

	public PlayerProductPurchaseInfo[] PlayerProductPurchaseInfos { get; set; }

	public WorldMapEventTimeInfo[] WorldMapEventTimeInfos { get; set; }

	public WorldMapEventInfo[] WorldMapEventInfos { get; set; }

	public WantedQuestInfo[] WantedQuestInfos { get; set; }

	public AttendanceInfo[] AttendanceInfos { get; set; }

	public RaidInfo[] RaidInfos { get; set; }

	public DeckInfo[] DeckInfos { get; set; }

	public CraftSlotInfo[] CraftSlotInfos { get; set; }

	public CostumeInfo[] CostumeInfos { get; set; }

	public PlayerAccessoryCostumeInfo[] PlayerAccessoryCostumeInfos { get; set; }

	public WeaponCostumeInfo[] WeaponCostumeInfos { get; set; }

	public HairCostumeInfo[] HairCostumeInfos { get; set; }

	public CostumeStorageSlotInfo[] CostumeStorageSlotInfos { get; set; }

	public PlayerAvatarHeroInfo PlayerAvatarHeroInfo { get; set; }

	public PurchaseMarketingInfo[] PurchaseMarketingInfos { get; set; }

	public PlayerItemUseInfo[] PlayerItemUseInfos { get; set; }

	public HideoutDungeonInfo[] HideoutDungeons { get; set; }

	public ConquestDungeonInfo[] ConquestDungeons { get; set; }

	public GodkingTrialDungeonInfo[] GodkingTrialDungeons { get; set; }

	public UnderPrisonInfo[] UnderPrisonInfos { get; set; }

	public PlayRecordInfo[] PlayRecordInfos { get; set; }

	public LoginDailyInfo[] LoginDailyInfos { get; set; }

	public PlayerArchiveInfo[] PlayerArchiveInfos { get; set; }

	public PlayerCurrencyInfo[] PlayerCurrencyInfos { get; set; }

	public NewbieMissionInfo[] NewbieMissionInfos { get; set; }

	public Notice[] Notices { get; set; }

	public DropBonusEvent[] DropBonusEvents { get; set; }

	public PayShopItemEventInfo[] PayShopItemEventInfos { get; set; }

	public PayShopImageUrlInfo[] ImageInfos { get; set; }

	public FreeEquipGachaInfo[] freeEquipGachaInfos { get; set; }

	public EquipGachaInfo[] EquipGachaInfos { get; set; }

	public ChapterRewardInfo[] chapterRewardInfos { get; set; }

	public StaminaResultInfo[] StaminaResults { get; set; }

	public ContentsStatusInfo[] ContentsStatuses { get; set; }

	public ContentsValueInfo[] ContentsValues { get; set; }

	public ContentsNoticePopupInfo[] ContentsNoticeInfos { get; set; }

	public PatchNoteInfo PatchNote { get; set; }

	public PayShopProductInfo[] PayShopProductInfos { get; set; }

	public PlayerInAppPurchaseInfo LatestIAPInfo { get; set; }

	public PlayerInAppPurchaseInfo[] NotCompletedIAPInfos { get; set; }

	public PlayerHeroFriendlyInfo HeroFriendlyInfo { get; set; }

	public FriendshipPointInfo[] SendRecvFriendshipPointInfos { get; set; }

	public FriendshipPointInfo[] SentFriendshipPointInfos { get; set; }

	public PlayerGuildPointInfo GuildPointInfo { get; set; }

	public MyGuildInfo GuildInfo { get; set; }

	public GuildArenaSessionInfo[] GuildArenaSessionInfo { get; set; }

	public NPCFriendlyInfo[] NPCFriendlyInfos { get; set; }

	public ChatBanInfo[] ChatBanInfos { get; set; }

	public string CreatedTime { get; set; }

	public WebNoticeInfo[] WebNotice { get; set; }

	public string GuildContentsRestrictedTime { get; set; }

	public bool AutoOpenInGameEvent { get; set; }

	public string ServerName { get; set; }

	public bool IsTutorialSkip { get; set; }

	public int UseRenewalPayShop { get; set; }

	public ClassBuffPointInfo[] ClassBuffPointInfos { get; set; }

	public ClassBuffInfo[] ClassBuffInfos { get; set; }

	public DispatchBattleInfo[] DispatchBattleInfos { get; set; }

	public RecordsOfHonorTopInfo[] RecordsOfHonorTopInfos { get; set; }

	public MonthlyHeroInfo[] MonthlyHeroInfos { get; set; }

	public SubQuestInfo[] SubQuestInfos { get; set; }

	public MainQuestInfo MainQuestInfo { get; set; }

	public EclipseDungeonInfo[] EclipseDungeonInfos { get; set; }

	public ClearMissionInfo[] ClearMissionInfos { get; set; }

	public string SignOutDoneTime { get; set; }

	public string AppGuardAuthId { get; set; }

	public string LastLoginTime { get; set; }

	public string InactiveTime { get; set; }

	public int RestoreAccountTimeSec { get; set; }

	public PlayerAnyMisc[] PlayerAnyMiscs { get; set; }

	public UserInfo[] DuplicateUserInfos { get; set; }
}
