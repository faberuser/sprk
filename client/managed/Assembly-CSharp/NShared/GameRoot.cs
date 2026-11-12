using System;
using System.Collections;
using MiniJSON;
using NShared.NCampaign.NCampaignState;
using NShared.NEvent;
using NShared.NJit;
using NShared.NUtil;
using NShared.NWorldMap;
using NVespa;

namespace NShared;

public class GameRoot
{
	public static ILocalizeProxy LocalizeProxy = LocalizeProxyBuilder.CreateDefault();

	private static GameRoot _instance;

	public static GameRoot Instance => GetInstance();

	public static bool isValidInstance => _instance != null;

	public IJsonLoader JsonLoader { get; private set; }

	public TaskManager TaskManager { get; private set; }

	public OperationRuntimeFactory OperationRuntimeFactory { get; private set; }

	public OperationConditionFactory OperationConditionFactory { get; private set; }

	public OperationConditionActionFactory OperationConditionActionFactory { get; private set; }

	public OperationGroupConditionFactory OperationGroupConditionFactory { get; private set; }

	public OperationModifierContainer OperationModifierContainer { get; private set; }

	public StateActionFactory StateActionFactory { get; private set; }

	public BattleLogFactory BattleLogFactory { get; private set; }

	public AiScriptContainer AiScriptContainer { get; private set; }

	public WinDeciderFactory WinDeciderFactory { get; private set; }

	public EventFactory EventFactory { get; private set; }

	public Action<Action, Action> AsyncRun { get; set; }

	public ExponentialTable ExponentialTableK { get; private set; }

	public ExponentialTable ExponentialTableM { get; private set; }

	public bool SnapshotEnabled { get; set; }

	public LocalizationValueDataContainer LocalizationValueDataContainer => TableRoot.Instance.GetDataContainer<LocalizationValueDataContainer>();

	public ItemDataContainer ItemDataContainer => TableRoot.Instance.GetDataContainer<ItemDataContainer>();

	public AccessorySelectItemDataContainer AccessorySelectItemDataContainer => TableRoot.Instance.GetDataContainer<AccessorySelectItemDataContainer>();

	public AwakenStoneDataContainer AwakenStoneDataContainer => TableRoot.Instance.GetDataContainer<AwakenStoneDataContainer>();

	public BoosterItemDataContainer BoosterItemDataContainer => TableRoot.Instance.GetDataContainer<BoosterItemDataContainer>();

	public CostumeSelectItemDataContainer CostumeSelectItemDataContainer => TableRoot.Instance.GetDataContainer<CostumeSelectItemDataContainer>();

	public CinemaItemDataContainer CinemaItemDataContainer => TableRoot.Instance.GetDataContainer<CinemaItemDataContainer>();

	public EquipEnchantOptionItemDataContainer EquipEnchantOptionItemDataContainer => TableRoot.Instance.GetDataContainer<EquipEnchantOptionItemDataContainer>();

	public EquipItemDataContainer EquipItemDataContainer => TableRoot.Instance.GetDataContainer<EquipItemDataContainer>();

	public FlaskItemDataContainer FlaskItemDataContainer => TableRoot.Instance.GetDataContainer<FlaskItemDataContainer>();

	public HeroGrowthItemDataContainer HeroGrowthItemDataContainer => TableRoot.Instance.GetDataContainer<HeroGrowthItemDataContainer>();

	public HeroLimitBreakExpItemDataContainer HeroLimitBreakExpItemDataContainer => TableRoot.Instance.GetDataContainer<HeroLimitBreakExpItemDataContainer>();

	public HeroSelectItemDataContainer HeroSelectItemDataContainer => TableRoot.Instance.GetDataContainer<HeroSelectItemDataContainer>();

	public MultipleHeroSelectItemDataContainer MultipleHeroSelectItemDataContainer => TableRoot.Instance.GetDataContainer<MultipleHeroSelectItemDataContainer>();

	public NPCFriendlyPointItemDataContainer NPCFriendlyPointItemDataContainer => TableRoot.Instance.GetDataContainer<NPCFriendlyPointItemDataContainer>();

	public PackageItemDataContainer PackageItemDataContainer => TableRoot.Instance.GetDataContainer<PackageItemDataContainer>();

	public PetSelectItemDataContainer PetSelectItemDataContainer => TableRoot.Instance.GetDataContainer<PetSelectItemDataContainer>();

	public PotionItemDataContainer PotionItemDataContainer => TableRoot.Instance.GetDataContainer<PotionItemDataContainer>();

	public RuneItemDataContainer RuneItemDataContainer => TableRoot.Instance.GetDataContainer<RuneItemDataContainer>();

	public SelectItemDataContainer SelectItemDataContainer => TableRoot.Instance.GetDataContainer<SelectItemDataContainer>();

	public UpgradeBookItemDataContainer UpgradeBookItemDataContainer => TableRoot.Instance.GetDataContainer<UpgradeBookItemDataContainer>();

	public WeaponUniqueSelectItemDataContainer WeaponUniqueSelectItemDataContainer => TableRoot.Instance.GetDataContainer<WeaponUniqueSelectItemDataContainer>();

	public GachaProbabilityDataContainer GachaProbabilityDataContainer => TableRoot.Instance.GetDataContainer<GachaProbabilityDataContainer>();

	public HeroAwakeDataContainer HeroAwakeDataContainer => TableRoot.Instance.GetDataContainer<HeroAwakeDataContainer>();

	public ItemGroupDataContainer ItemGroupDataContainer => TableRoot.Instance.GetDataContainer<ItemGroupDataContainer>();

	public RewardDataContainer RewardDataContainer => TableRoot.Instance.GetDataContainer<RewardDataContainer>();

	public CreatureStarGradeStatDataContainer CreatureStarGradeStatDataContainer => TableRoot.Instance.GetDataContainer<CreatureStarGradeStatDataContainer>();

	public EquipUpgradeStatDataContainer EquipUpgradeStatDataContainer => TableRoot.Instance.GetDataContainer<EquipUpgradeStatDataContainer>();

	public StateDataContainer StateDataContainer => TableRoot.Instance.GetDataContainer<StateDataContainer>();

	public MonsterTierDataContainer MonsterTierDataContainer => TableRoot.Instance.GetDataContainer<MonsterTierDataContainer>();

	public AttendanceRewardUrlDataContainer AttendanceRewardUrlDataContainer => TableRoot.Instance.GetDataContainer<AttendanceRewardUrlDataContainer>();

	public AchievementDataContainer AchievementDataContainer => TableRoot.Instance.GetDataContainer<AchievementDataContainer>();

	public CreatureDataContainer CreatureDataContainer => TableRoot.Instance.GetDataContainer<CreatureDataContainer>();

	public EquipOptionDataContainer EquipOptionDataContainer => TableRoot.Instance.GetDataContainer<EquipOptionDataContainer>();

	public CostumeDataContainer CostumeDataContainer => TableRoot.Instance.GetDataContainer<CostumeDataContainer>();

	public CampaignNodeDataContainer CampaignNodeDataContainer => TableRoot.Instance.GetDataContainer<CampaignNodeDataContainer>();

	public CostumeGroupDataContainer CostumeGroupDataContainer => TableRoot.Instance.GetDataContainer<CostumeGroupDataContainer>();

	public AccessoryCostumeDataContainer AccessoryCostumeDataContainer => TableRoot.Instance.GetDataContainer<AccessoryCostumeDataContainer>();

	public AccessoryCostumeGroupDataContainer AccessoryCostumeGroupDataContainer => TableRoot.Instance.GetDataContainer<AccessoryCostumeGroupDataContainer>();

	public HairCostumeDataContainer HairCostumeDataContainer => TableRoot.Instance.GetDataContainer<HairCostumeDataContainer>();

	public WeaponCostumeDataContainer WeaponCostumeDataContainer => TableRoot.Instance.GetDataContainer<WeaponCostumeDataContainer>();

	public MainStoryDataContainer MainStoryDataContainer => TableRoot.Instance.GetDataContainer<MainStoryDataContainer>();

	public ShopDataContainer ShopDataContainer => TableRoot.Instance.GetDataContainer<ShopDataContainer>();

	public SkillDataContainer SkillDataContainer => TableRoot.Instance.GetDataContainer<SkillDataContainer>();

	public CampaignWaveDataContainer CampaignWaveDataContainer => TableRoot.Instance.GetDataContainer<CampaignWaveDataContainer>();

	public CensoredWordDataContainer CensoredWordDataContainer => TableRoot.Instance.GetDataContainer<CensoredWordDataContainer>();

	public CampaignDungeonDataContainer CampaignDungeonDataContainer => TableRoot.Instance.GetDataContainer<CampaignDungeonDataContainer>();

	public ItemProbabilityDataContainer ItemProbabilityDataContainer => TableRoot.Instance.GetDataContainer<ItemProbabilityDataContainer>();

	public AchievementTextDataContainer AchievementTextDataContainer => TableRoot.Instance.GetDataContainer<AchievementTextDataContainer>();

	public EquipUpgradeExpDataContainer EquipUpgradeExpDataContainer => TableRoot.Instance.GetDataContainer<EquipUpgradeExpDataContainer>();

	public TutorialDataContainer TutorialDataContainer => TableRoot.Instance.GetDataContainer<TutorialDataContainer>();

	public HeroAnimationViewDataContainer HeroAnimationViewDataContainer => TableRoot.Instance.GetDataContainer<HeroAnimationViewDataContainer>();

	public SubQuestDataContainer SubQuestDataContainer => TableRoot.Instance.GetDataContainer<SubQuestDataContainer>();

	public ShopItemDataContainer ShopItemDataContainer => TableRoot.Instance.GetDataContainer<ShopItemDataContainer>();

	public EventTriggerDataContainer EventTriggerDataContainer => TableRoot.Instance.GetDataContainer<EventTriggerDataContainer>();

	public ClientCreatureDataContainer ClientCreatureDataContainer => TableRoot.Instance.GetDataContainer<ClientCreatureDataContainer>();

	public AffixDataContainer AffixDataContainer => TableRoot.Instance.GetDataContainer<AffixDataContainer>();

	public LoadingDataContainer LoadingDataContainer => TableRoot.Instance.GetDataContainer<LoadingDataContainer>();

	public RaidDataContainer RaidDataContainer => TableRoot.Instance.GetDataContainer<RaidDataContainer>();

	public MissionVisualDataContainer MissionVisualDataContainer => TableRoot.Instance.GetDataContainer<MissionVisualDataContainer>();

	public CampaignTacticDataContainer CampaignTacticDataContainer => TableRoot.Instance.GetDataContainer<CampaignTacticDataContainer>();

	public SoulWeaponAbilityRewardDataContainer SoulWeaponAbilityTicketRewardDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponAbilityRewardDataContainer>();

	public EquipUpgradeTierDataContainer EquipUpgradeTierDataContainer => TableRoot.Instance.GetDataContainer<EquipUpgradeTierDataContainer>();

	public SoulBreakDataContainer SoulBreakDataContainer => TableRoot.Instance.GetDataContainer<SoulBreakDataContainer>();

	public ClientCustomizingPivotDataContainer ClientCustomizingPivotDataContainer => TableRoot.Instance.GetDataContainer<ClientCustomizingPivotDataContainer>();

	public ClassBuffDataContainer ClassBuffDataContainer => TableRoot.Instance.GetDataContainer<ClassBuffDataContainer>();

	public EntryMenuSettingDataContainer EntryMenuSettingDataContainer => TableRoot.Instance.GetDataContainer<EntryMenuSettingDataContainer>();

	public EquipUpgradePriceDataContainer EquipUpgradePriceDataContainer => TableRoot.Instance.GetDataContainer<EquipUpgradePriceDataContainer>();

	public GuildSuppressSessionDataContainer GuildSuppressSessionDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressSessionDataContainer>();

	public HeroFriendlyActionDataContainer HeroFriendlyActionDataContainer => TableRoot.Instance.GetDataContainer<HeroFriendlyActionDataContainer>();

	public HeroLimitBreakDataContainer HeroLimitBreakDataContainer => TableRoot.Instance.GetDataContainer<HeroLimitBreakDataContainer>();

	public HeroVoiceDataContainer HeroVoiceDataContainer => TableRoot.Instance.GetDataContainer<HeroVoiceDataContainer>();

	public EventDungeonDataContainer EventDungeonDataContainer => TableRoot.Instance.GetDataContainer<EventDungeonDataContainer>();

	public CampaignChapterDataContainer CampaignChapterDataContainer => TableRoot.Instance.GetDataContainer<CampaignChapterDataContainer>();

	public NPCDataContainer NPCDataContainer => TableRoot.Instance.GetDataContainer<NPCDataContainer>();

	public KingsPassSeasonMissionDataContainer KingsPassSeasonMissionDataContainer => TableRoot.Instance.GetDataContainer<KingsPassSeasonMissionDataContainer>();

	public ChallengeRaidRewardDataContainer ChallengeRaidRewardDataContainer => TableRoot.Instance.GetDataContainer<ChallengeRaidRewardDataContainer>();

	public BanRuleDataContainer BanRuleDataContainer => TableRoot.Instance.GetDataContainer<BanRuleDataContainer>();

	public GuildSuppressDungeonDataContainer GuildSuppressDungeonDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressDungeonDataContainer>();

	public SubStoryDataContainer SubStoryDataContainer => TableRoot.Instance.GetDataContainer<SubStoryDataContainer>();

	public PartyDungeonDataContainer PartyDungeonDataContainer => TableRoot.Instance.GetDataContainer<PartyDungeonDataContainer>();

	public AttendanceRewardInfoDataContainer AttendanceRewardInfoDataContainer => TableRoot.Instance.GetDataContainer<AttendanceRewardInfoDataContainer>();

	public EquipTransitionTicketDataContainer EquipTransitionTicketDataContainer => TableRoot.Instance.GetDataContainer<EquipTransitionTicketDataContainer>();

	public HeroFriendlyDataContainer HeroFriendlyDataContainer => TableRoot.Instance.GetDataContainer<HeroFriendlyDataContainer>();

	public CreatureLevelFactorDataContainer CreatureLevelFactorDataContainer => TableRoot.Instance.GetDataContainer<CreatureLevelFactorDataContainer>();

	public ValanceCraftDataContainer ValanceCraftDataContainer => TableRoot.Instance.GetDataContainer<ValanceCraftDataContainer>();

	public PurchaseMarketingPopupDataContainer PurchaseMarketingPopupDataContainer => TableRoot.Instance.GetDataContainer<PurchaseMarketingPopupDataContainer>();

	public SoulWeaponNodeTreeDataContainer SoulWeaponNodeTreeDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponNodeTreeDataContainer>();

	public StepUpGachaRewardDataContainer StepUpGachaRewardDataContainer => TableRoot.Instance.GetDataContainer<StepUpGachaRewardDataContainer>();

	public CreatureVisualDataContainer CreatureVisualDataContainer => TableRoot.Instance.GetDataContainer<CreatureVisualDataContainer>();

	public BattleDefineDataContainer BattleDefineDataContainer => TableRoot.Instance.GetDataContainer<BattleDefineDataContainer>();

	public EquipItemBreakDataContainer EquipItemBreakDataContainer => TableRoot.Instance.GetDataContainer<EquipItemBreakDataContainer>();

	public NewPayShopTagDataContainer NewPayShopTagDataContainer => TableRoot.Instance.GetDataContainer<NewPayShopTagDataContainer>();

	public HeroProfileDataContainer HeroProfileDataContainer => TableRoot.Instance.GetDataContainer<HeroProfileDataContainer>();

	public ChallengeRaidClearRewardDataContainer ChallengeRaidClearRewardDataContainer => TableRoot.Instance.GetDataContainer<ChallengeRaidClearRewardDataContainer>();

	public PortalRenewalDataContainer PortalRenewalDataContainer => TableRoot.Instance.GetDataContainer<PortalRenewalDataContainer>();

	public HelpListDataContainer HelpListDataContainer => TableRoot.Instance.GetDataContainer<HelpListDataContainer>();

	public TeamLevelDataContainer TeamLevelDataContainer => TableRoot.Instance.GetDataContainer<TeamLevelDataContainer>();

	public SkillLevelFactorDataContainer SkillLevelFactorDataContainer => TableRoot.Instance.GetDataContainer<SkillLevelFactorDataContainer>();

	public SoulWeaponUpgradePriceDataContainer SoulWeaponUpgradePriceDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponUpgradePriceDataContainer>();

	public TowerFloorDataContainer TowerFloorDataContainer => TableRoot.Instance.GetDataContainer<TowerFloorDataContainer>();

	public PunishmentRuneItemCountDataContainer PunishmentRuneItemCountDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRuneItemCountDataContainer>();

	public NewEquipGachaDataContainer NewEquipGachaDataContainer => TableRoot.Instance.GetDataContainer<NewEquipGachaDataContainer>();

	public ChallengeRaidDataContainer ChallengeRaidDataContainer => TableRoot.Instance.GetDataContainer<ChallengeRaidDataContainer>();

	public EquipAwakenPriceDataContainer EquipAwakenPriceDataContainer => TableRoot.Instance.GetDataContainer<EquipAwakenPriceDataContainer>();

	public CraftItemDataContainer CraftItemDataContainer => TableRoot.Instance.GetDataContainer<CraftItemDataContainer>();

	public ValanceEquipFilterDataContainer ValanceEquipFilterDataContainer => TableRoot.Instance.GetDataContainer<ValanceEquipFilterDataContainer>();

	public ExpressionDataContainer ExpressionDataContainer => TableRoot.Instance.GetDataContainer<ExpressionDataContainer>();

	public EclipseRewardDataContainer EclipseRewardDataContainer => TableRoot.Instance.GetDataContainer<EclipseRewardDataContainer>();

	public ValanceAwakenDataContainer ValanceAwakenDataContainer => TableRoot.Instance.GetDataContainer<ValanceAwakenDataContainer>();

	public SkillPriceDataContainer SkillPriceDataContainer => TableRoot.Instance.GetDataContainer<SkillPriceDataContainer>();

	public AccountBuffDataContainer AccountBuffDataContainer => TableRoot.Instance.GetDataContainer<AccountBuffDataContainer>();

	public AdjustTokenDataContainer AdjustTokenDataContainer => TableRoot.Instance.GetDataContainer<AdjustTokenDataContainer>();

	public ConstantDataContainer ConstantDataContainer => TableRoot.Instance.GetDataContainer<ConstantDataContainer>();

	public CountryDataContainer CountryDataContainer => TableRoot.Instance.GetDataContainer<CountryDataContainer>();

	public CountryServerDataContainer CountryServerDataContainer => TableRoot.Instance.GetDataContainer<CountryServerDataContainer>();

	public LanguageDataContainer LanguageDataContainer => TableRoot.Instance.GetDataContainer<LanguageDataContainer>();

	public LanguagePatternDataContainer LanguagePatternDataContainer => TableRoot.Instance.GetDataContainer<LanguagePatternDataContainer>();

	public SubStoryEventDataContainer SubStoryEventDataContainer => TableRoot.Instance.GetDataContainer<SubStoryEventDataContainer>();

	public CodecSupportListDataContainer CodecSupportListContainer => TableRoot.Instance.GetDataContainer<CodecSupportListDataContainer>();

	public CreatureOverlapDataContainer CreatureOverlapContainer => TableRoot.Instance.GetDataContainer<CreatureOverlapDataContainer>();

	public CutSceneForegroundDataContainer CutSceneForegroundDataContainer => TableRoot.Instance.GetDataContainer<CutSceneForegroundDataContainer>();

	public CreatureLevelStatDataContainer2 CreatureLevelStatDataContainer2 => TableRoot.Instance.GetDataContainer<CreatureLevelStatDataContainer2>();

	public CreatureStatDataContainer CreatureStatDataContainer => TableRoot.Instance.GetDataContainer<CreatureStatDataContainer>();

	public CreatureLevelDataContainer CreatureLevelDataContainer => TableRoot.Instance.GetDataContainer<CreatureLevelDataContainer>();

	public CreatureStarDataContainer CreatureStarDataContainer => TableRoot.Instance.GetDataContainer<CreatureStarDataContainer>();

	public CreatureStarStatDataContainer CreatureStarStatDataContainer => TableRoot.Instance.GetDataContainer<CreatureStarStatDataContainer>();

	public CreatureGradeDataContainer CreatureGradeDataContainer => TableRoot.Instance.GetDataContainer<CreatureGradeDataContainer>();

	public StateActionDataContainer StateActionDataContainer => TableRoot.Instance.GetDataContainer<StateActionDataContainer>();

	public ThreatDataContainer ThreatDataContainer => TableRoot.Instance.GetDataContainer<ThreatDataContainer>();

	public CreatureTranscendOptionDataContainer CreatureTranscendOptionDataContainer => TableRoot.Instance.GetDataContainer<CreatureTranscendOptionDataContainer>();

	public DefenceLevelDataContainer DefenceLevelDataContainer => TableRoot.Instance.GetDataContainer<DefenceLevelDataContainer>();

	public DbLogDataContainer DbLogDataContainer => TableRoot.Instance.GetDataContainer<DbLogDataContainer>();

	public EquipOptionGroupDataContainer EquipOptionGroupDataContainer => TableRoot.Instance.GetDataContainer<EquipOptionGroupDataContainer>();

	public EquipOptionUpgradeDataContainer EquipOptionUpgradeDataContainer => TableRoot.Instance.GetDataContainer<EquipOptionUpgradeDataContainer>();

	public EquipOptionChangeDataContainer EquipOptionChangeDataContainer => TableRoot.Instance.GetDataContainer<EquipOptionChangeDataContainer>();

	public EquipItemDetailDataContainer EquipItemDetailDataContainer => TableRoot.Instance.GetDataContainer<EquipItemDetailDataContainer>();

	public EquipItemStarDataContainer EquipItemStarDataContainer => TableRoot.Instance.GetDataContainer<EquipItemStarDataContainer>();

	public EquipItemLevelDataContainer EquipItemLevelDataContainer => TableRoot.Instance.GetDataContainer<EquipItemLevelDataContainer>();

	public EquipAwakenPointDataContainer EquipAwakenPointDataContainer => TableRoot.Instance.GetDataContainer<EquipAwakenPointDataContainer>();

	public EquipAwakenRatioDataContainer EquipAwakenRatioDataContainer => TableRoot.Instance.GetDataContainer<EquipAwakenRatioDataContainer>();

	public EquipSetBonusDataContainer EquipSetBonusDataContainer => TableRoot.Instance.GetDataContainer<EquipSetBonusDataContainer>();

	public EquipSlotDataContainer EquipSlotDataContainer => TableRoot.Instance.GetDataContainer<EquipSlotDataContainer>();

	public ItemGradeDataContainer ItemGradeDataContainer => TableRoot.Instance.GetDataContainer<ItemGradeDataContainer>();

	public PatchDownloadSpecDataContainer PatchDownloadSpecDataContainer => TableRoot.Instance.GetDataContainer<PatchDownloadSpecDataContainer>();

	public SoulWeaponAbilityTicketDataContainer SoulWeaponAbilityTicketDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponAbilityTicketDataContainer>();

	public CSMailDataContainer CSMailDataContainer => TableRoot.Instance.GetDataContainer<CSMailDataContainer>();

	public SkillProjectileDataContainer SkillProjectileDataContainer => TableRoot.Instance.GetDataContainer<SkillProjectileDataContainer>();

	public SkillProjectileGroupDataContainer SkillProjectileGroupDataContainer => TableRoot.Instance.GetDataContainer<SkillProjectileGroupDataContainer>();

	public StatAttributeDataContainer StatAttributeDataContainer => TableRoot.Instance.GetDataContainer<StatAttributeDataContainer>();

	public StateToolTipDataContainer StateToolTipDataContainer => TableRoot.Instance.GetDataContainer<StateToolTipDataContainer>();

	public CampaignNodeNPCDataContainer CampaignNodeNPCDataContainer => TableRoot.Instance.GetDataContainer<CampaignNodeNPCDataContainer>();

	public CampaignFieldDataContainer CampaignFieldDataContainer => TableRoot.Instance.GetDataContainer<CampaignFieldDataContainer>();

	public CampaignMultiAvatarDataContainer CampaignMultiAvatarDataContainer => TableRoot.Instance.GetDataContainer<CampaignMultiAvatarDataContainer>();

	public PortalDataContainer PortalDataContainer => TableRoot.Instance.GetDataContainer<PortalDataContainer>();

	public TowerDataContainer TowerDataContainer => TableRoot.Instance.GetDataContainer<TowerDataContainer>();

	public MazeInfoDataContainer MazeInfoDataContainer => TableRoot.Instance.GetDataContainer<MazeInfoDataContainer>();

	public MazeClearRewardDataContainer MazeClearRewardDataContainer => TableRoot.Instance.GetDataContainer<MazeClearRewardDataContainer>();

	public GuildBuildingDataContainer GuildBuildingDataContainer => TableRoot.Instance.GetDataContainer<GuildBuildingDataContainer>();

	public GuildLevelDataContainer GuildLevelDataContainer => TableRoot.Instance.GetDataContainer<GuildLevelDataContainer>();

	public GuildSkillDataContainer GuildSkillDataContainer => TableRoot.Instance.GetDataContainer<GuildSkillDataContainer>();

	public GuildSkillLevelDataContainer GuildSkillLevelDataContainer => TableRoot.Instance.GetDataContainer<GuildSkillLevelDataContainer>();

	public GuildContributeRewardDataContainer GuildContributeRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildContributeRewardDataContainer>();

	public GuildAttendanceRewardDataContainer GuildAttendanceRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildAttendanceRewardDataContainer>();

	public MatchTierDataContainer MatchTierDataContainer => TableRoot.Instance.GetDataContainer<MatchTierDataContainer>();

	public MatchRewardDataContainer MatchRewardDataContainer => TableRoot.Instance.GetDataContainer<MatchRewardDataContainer>();

	public LuckyArenaRewardDataContainer LuckyArenaRewardDataContainer => TableRoot.Instance.GetDataContainer<LuckyArenaRewardDataContainer>();

	public GlobalBanPickTierDataContainer GlobalBanPickTierDataContainer => TableRoot.Instance.GetDataContainer<GlobalBanPickTierDataContainer>();

	public GlobalBanPickTierRewardDataContainer GlobalBanPickTierRewardDataContainer => TableRoot.Instance.GetDataContainer<GlobalBanPickTierRewardDataContainer>();

	public MatchSeasonDataContainer MatchSeasonDataContainer => TableRoot.Instance.GetDataContainer<MatchSeasonDataContainer>();

	public MatchBanPickStateDataContainer MatchBanPickStateDataContainer => TableRoot.Instance.GetDataContainer<MatchBanPickStateDataContainer>();

	public MatchBanPickSeasonDataContainer MatchBanPickSeasonDataContainer => TableRoot.Instance.GetDataContainer<MatchBanPickSeasonDataContainer>();

	public LuckyArenaDeckPresetDataContainer LuckyArenaDeckPresetDataContainer => TableRoot.Instance.GetDataContainer<LuckyArenaDeckPresetDataContainer>();

	public DOWDungeonDataContainer DOWDungeonDataContainer => TableRoot.Instance.GetDataContainer<DOWDungeonDataContainer>();

	public DOWDungeonTextDataContainer DOWDungeonTextDataContainer => TableRoot.Instance.GetDataContainer<DOWDungeonTextDataContainer>();

	public MemoryFragmentDataContainer MemoryFragmentDataContainer => TableRoot.Instance.GetDataContainer<MemoryFragmentDataContainer>();

	public EventDataContainer WorldMapEventDataContainer => TableRoot.Instance.GetDataContainer<EventDataContainer>();

	public SelectRewardDataContainer SelectRewardDataContainer => TableRoot.Instance.GetDataContainer<SelectRewardDataContainer>();

	public CreatureAwakeChallengeItemDataContainer CreatureAwakeChallengeItemDataContainer => TableRoot.Instance.GetDataContainer<CreatureAwakeChallengeItemDataContainer>();

	public WantedQuestDataContainer WantedQuestDataContainer => TableRoot.Instance.GetDataContainer<WantedQuestDataContainer>();

	public WantedPathDataContainer WantedPathDataContainer => TableRoot.Instance.GetDataContainer<WantedPathDataContainer>();

	public CreatureStarPriceDataContainer CreatureStarPriceDataContainer => TableRoot.Instance.GetDataContainer<CreatureStarPriceDataContainer>();

	public ClientStatDisplayScoreDataContainer ClientStatDisplayScoreDataContainer => TableRoot.Instance.GetDataContainer<ClientStatDisplayScoreDataContainer>();

	public StatAttenuationDataContainer StatAttenuationDataContainer => TableRoot.Instance.GetDataContainer<StatAttenuationDataContainer>();

	public AddedStatAttenuationDataContainer AddedStatAttenuationDataContainer => TableRoot.Instance.GetDataContainer<AddedStatAttenuationDataContainer>();

	public MultedStatAttenuationDataContainer MultedStatAttenuationDataContainer => TableRoot.Instance.GetDataContainer<MultedStatAttenuationDataContainer>();

	public WorldMapPropDataContainer WorldMapPropDataContainer => TableRoot.Instance.GetDataContainer<WorldMapPropDataContainer>();

	public WorldMapBuildDataContainer WorldMapBuildDataContainer => TableRoot.Instance.GetDataContainer<WorldMapBuildDataContainer>();

	public PortalPropDataContainer PortalPropDataContainer => TableRoot.Instance.GetDataContainer<PortalPropDataContainer>();

	public NewPayShopBannerDataContainer NewPayShopBannerDataContainer => TableRoot.Instance.GetDataContainer<NewPayShopBannerDataContainer>();

	public NewPayShopOpenConditionDataContainer NewPayShopOpenConditionDataContainer => TableRoot.Instance.GetDataContainer<NewPayShopOpenConditionDataContainer>();

	public StaminaDataContainer StaminaDataContainer => TableRoot.Instance.GetDataContainer<StaminaDataContainer>();

	public RaidMultiInfoDataContainer RaidMultiInfoDataContainer => TableRoot.Instance.GetDataContainer<RaidMultiInfoDataContainer>();

	public GameGuideDataContainer GameGuideDataContainer => TableRoot.Instance.GetDataContainer<GameGuideDataContainer>();

	public GameMiniGuideDataContainer GameMiniGuideDataContainer => TableRoot.Instance.GetDataContainer<GameMiniGuideDataContainer>();

	public BottomButtonDataContainer BottomButtonDataContainer => TableRoot.Instance.GetDataContainer<BottomButtonDataContainer>();

	public LeftMenuButtonDataContainer LeftMenuButtonDataContainer => TableRoot.Instance.GetDataContainer<LeftMenuButtonDataContainer>();

	public ArtifactRestoreDataContainer ArtifactRestoreDataContainer => TableRoot.Instance.GetDataContainer<ArtifactRestoreDataContainer>();

	public WorldBossDataContainer WorldBossDataContainer => TableRoot.Instance.GetDataContainer<WorldBossDataContainer>();

	public WorldBossDailyAchievementDataContainer WorldBossDailyAchievementDataContainer => TableRoot.Instance.GetDataContainer<WorldBossDailyAchievementDataContainer>();

	public EventWorldBossDataContainer EventWorldBossDataContainer => TableRoot.Instance.GetDataContainer<EventWorldBossDataContainer>();

	public WorldBossRewardDataContainer WorldBossRewardDataContainer => TableRoot.Instance.GetDataContainer<WorldBossRewardDataContainer>();

	public WorldBossDailyRewardDataContainer WorldBossDailyRewardDataContainer => TableRoot.Instance.GetDataContainer<WorldBossDailyRewardDataContainer>();

	public WorldBossScoreRewardDataContainer WorldBossScoreRewardDataContainer => TableRoot.Instance.GetDataContainer<WorldBossScoreRewardDataContainer>();

	public WorldBossServerRewardDataContainer WorldBossServerRewardDataContainer => TableRoot.Instance.GetDataContainer<WorldBossServerRewardDataContainer>();

	public WorldBossGlobalRewardDataContainer WorldBossGlobalRewardDataContainer => TableRoot.Instance.GetDataContainer<WorldBossGlobalRewardDataContainer>();

	public WorldBossTotalRankRewardDataContainer WorldBossTotalRankRewardDataContainer => TableRoot.Instance.GetDataContainer<WorldBossTotalRankRewardDataContainer>();

	public InspectUserDataContainer InspectUserDataContainer => TableRoot.Instance.GetDataContainer<InspectUserDataContainer>();

	public OptionSellDataContainer OptionSellDataContainer => TableRoot.Instance.GetDataContainer<OptionSellDataContainer>();

	public CraftSortDataContainer CraftSortDataContainer => TableRoot.Instance.GetDataContainer<CraftSortDataContainer>();

	public DeviceModelDataContainer DeviceModelDataContainer => TableRoot.Instance.GetDataContainer<DeviceModelDataContainer>();

	public CraftInstantGemDataContainer CraftInstantGemDataContainer => TableRoot.Instance.GetDataContainer<CraftInstantGemDataContainer>();

	public RuneItemBreakDataContainer RuneItemBreakDataContainer => TableRoot.Instance.GetDataContainer<RuneItemBreakDataContainer>();

	public PunishmentRuneItemBreakDataContainer PunishmentRuneItemBreakDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRuneItemBreakDataContainer>();

	public ShopItemGroupDataContainer ShopItemGroupDataContainer => TableRoot.Instance.GetDataContainer<ShopItemGroupDataContainer>();

	public ShopItemSortDataContainer ShopItemSortDataContainer => TableRoot.Instance.GetDataContainer<ShopItemSortDataContainer>();

	public ShopItemSortGroupDataContainer ShopItemSortGroupDataContainer => TableRoot.Instance.GetDataContainer<ShopItemSortGroupDataContainer>();

	public ShopItemFilterDataContainer ShopItemFilterDataContainer => TableRoot.Instance.GetDataContainer<ShopItemFilterDataContainer>();

	public GuildRaidChapterDataContainer GuildRaidChapterDataContainer => TableRoot.Instance.GetDataContainer<GuildRaidChapterDataContainer>();

	public GuildRaidDungeonDataContainer GuildRaidDungeonDataContainer => TableRoot.Instance.GetDataContainer<GuildRaidDungeonDataContainer>();

	public GuildRaidBonusDungeonDataContainer GuildRaidBonusDungeonDataContainer => TableRoot.Instance.GetDataContainer<GuildRaidBonusDungeonDataContainer>();

	public GuildRaidDummyDungeonDataContainer GuildRaidDummyDungeonDataContainer => TableRoot.Instance.GetDataContainer<GuildRaidDummyDungeonDataContainer>();

	public GuildRaidChapterRewardDataContainer GuildRaidChapterRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildRaidChapterRewardDataContainer>();

	public BoosterDefineDataContainer BoosterDefineDataContainer => TableRoot.Instance.GetDataContainer<BoosterDefineDataContainer>();

	public BoosterIconActionDataContainer BoosterIconActionDataContainer => TableRoot.Instance.GetDataContainer<BoosterIconActionDataContainer>();

	public NPCFriendlyActionDataContainer NPCFriendlyActionDataContainer => TableRoot.Instance.GetDataContainer<NPCFriendlyActionDataContainer>();

	public NPCFriendlyPointDataContainer NPCFriendlyPointDataContainer => TableRoot.Instance.GetDataContainer<NPCFriendlyPointDataContainer>();

	public PhaseStepRewardDataContainer PhaseStepRewardDataContainer => TableRoot.Instance.GetDataContainer<PhaseStepRewardDataContainer>();

	public HeroBonusDataContainer HeroBonusDataContainer => TableRoot.Instance.GetDataContainer<HeroBonusDataContainer>();

	public EquipEnchantableConditionDataContainer EquipEnchantableConditionDataContainer => TableRoot.Instance.GetDataContainer<EquipEnchantableConditionDataContainer>();

	public EventDungeonGroupDataContainer EventDungeonGroupDataContainer => TableRoot.Instance.GetDataContainer<EventDungeonGroupDataContainer>();

	public MainStoryChapterDataContainer MainStoryChapterDataContainer => TableRoot.Instance.GetDataContainer<MainStoryChapterDataContainer>();

	public CurrencyTypeDataContainer CurrencyTypeDataContainer => TableRoot.Instance.GetDataContainer<CurrencyTypeDataContainer>();

	public PurchaseMarketingDataContainer PurchaseMarketingDataContainer => TableRoot.Instance.GetDataContainer<PurchaseMarketingDataContainer>();

	public SimilarAlphabetDataContainer SimilarAlphabetDataContainer => TableRoot.Instance.GetDataContainer<SimilarAlphabetDataContainer>();

	public AttendanceInfoDataContainer AttendanceInfoDataContainer => TableRoot.Instance.GetDataContainer<AttendanceInfoDataContainer>();

	public AttendanceUrlDataContainer AttendanceUrlDataContainer => TableRoot.Instance.GetDataContainer<AttendanceUrlDataContainer>();

	public NewPayShopGroupDataContainer NewPayShopGroupDataContainer => TableRoot.Instance.GetDataContainer<NewPayShopGroupDataContainer>();

	public EmoticonDataContainer EmoticonDataContainer => TableRoot.Instance.GetDataContainer<EmoticonDataContainer>();

	public EmoticonTabDataContainer EmotionTabDataContainer => TableRoot.Instance.GetDataContainer<EmoticonTabDataContainer>();

	public PlayRecordDataContainer PlayRecordDataContainer => TableRoot.Instance.GetDataContainer<PlayRecordDataContainer>();

	public AccumulateLoginRewardDataContainer AccumulateLoginRewardDataContainer => TableRoot.Instance.GetDataContainer<AccumulateLoginRewardDataContainer>();

	public PlayerArchiveDataContainer PlayerArchiveDataContainer => TableRoot.Instance.GetDataContainer<PlayerArchiveDataContainer>();

	public RouletteDataContainer RouletteDataContainer => TableRoot.Instance.GetDataContainer<RouletteDataContainer>();

	public RewardItemIconOrderDataContainer RewardItemIconOrderDataContainer => TableRoot.Instance.GetDataContainer<RewardItemIconOrderDataContainer>();

	public AppReviewConditionDataContainer AppReviewConditionDataContainer => TableRoot.Instance.GetDataContainer<AppReviewConditionDataContainer>();

	public StoreCurrencyUnitDataContainer StoreCurrencyUnitDataContainer => TableRoot.Instance.GetDataContainer<StoreCurrencyUnitDataContainer>();

	public GuildSuppressRewardDataContainer GuildSuppressRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressRewardDataContainer>();

	public GuildSuppressServerRewardDataContainer GuildSuppressServerRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressServerRewardDataContainer>();

	public GuildSuppressGlobalRewardDataContainer GuildSuppressGlobalRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressGlobalRewardDataContainer>();

	public GuildSuppressTotalRewardDataContainer GuildSuppressTotalRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressTotalRewardDataContainer>();

	public GuildSuppressChapterDataContainer GuildSuppressChapterDataContainer => TableRoot.Instance.GetDataContainer<GuildSuppressChapterDataContainer>();

	public GuildPenaltyDataContainer GuildPenaltyDataContainer => TableRoot.Instance.GetDataContainer<GuildPenaltyDataContainer>();

	public RelatedStatDataContainer RelatedStatDataContainer => TableRoot.Instance.GetDataContainer<RelatedStatDataContainer>();

	public PcAvatarDataContainer PcAvatarDataContainer => TableRoot.Instance.GetDataContainer<PcAvatarDataContainer>();

	public BossWarningDataContainer BossWarningDataContainer => TableRoot.Instance.GetDataContainer<BossWarningDataContainer>();

	public EventWorldBossSeasonDataContainer EventWorldBossSeasonDataContainer => TableRoot.Instance.GetDataContainer<EventWorldBossSeasonDataContainer>();

	public InventoryExtendDataContainer InventoryExtendDataContainer => TableRoot.Instance.GetDataContainer<InventoryExtendDataContainer>();

	public PlayGuideDataContainer PlayGuideDataContainer => TableRoot.Instance.GetDataContainer<PlayGuideDataContainer>();

	public StorageSlotExtendDataContainer StorageSlotExtendDataContainer => TableRoot.Instance.GetDataContainer<StorageSlotExtendDataContainer>();

	public AdventureNoticeDataContainer AdventureNoticeDataContainer => TableRoot.Instance.GetDataContainer<AdventureNoticeDataContainer>();

	public CustomEquipItemDataContainer CustomEquipItemDataContainer => TableRoot.Instance.GetDataContainer<CustomEquipItemDataContainer>();

	public CountryCodeDataContainer CountryCodeDataContainer => TableRoot.Instance.GetDataContainer<CountryCodeDataContainer>();

	public CountryCodeGroupDataContainer CountryCodeGroupDataContainer => TableRoot.Instance.GetDataContainer<CountryCodeGroupDataContainer>();

	public ServerGroupNameDataContainer ServerGroupNameDataContainer => TableRoot.Instance.GetDataContainer<ServerGroupNameDataContainer>();

	public GachaShopProbabilityDataContainer GachaShopProbabilityDataContainer => TableRoot.Instance.GetDataContainer<GachaShopProbabilityDataContainer>();

	public GameOptionDataContainer GameOptionDataContainer => TableRoot.Instance.GetDataContainer<GameOptionDataContainer>();

	public ContentsNoticePopupDataContainer ContentsNoticePopupDataContainer => TableRoot.Instance.GetDataContainer<ContentsNoticePopupDataContainer>();

	public TeamLevelCommonOptionOrderDataContainer TeamLevelCommonOptionOrderDataContainer => TableRoot.Instance.GetDataContainer<TeamLevelCommonOptionOrderDataContainer>();

	public TeamLevelBuffOptionDataContainer TeamLevelBuffOptionDataContainer => TableRoot.Instance.GetDataContainer<TeamLevelBuffOptionDataContainer>();

	public TeamLevelBonusBuffOptionDataContainer TeamLevelBonusBuffOptionDataContainer => TableRoot.Instance.GetDataContainer<TeamLevelBonusBuffOptionDataContainer>();

	public EquipFilterOptionGroupDataContainer EquipFilterOptionGroupDataContainer => TableRoot.Instance.GetDataContainer<EquipFilterOptionGroupDataContainer>();

	public EquipFilterOptionDataContainer EquipFilterOptionDataContainer => TableRoot.Instance.GetDataContainer<EquipFilterOptionDataContainer>();

	public DispatchStateDataContainer DispatchStateDataContainer => TableRoot.Instance.GetDataContainer<DispatchStateDataContainer>();

	public HonoraryChamberMenuDataContainer HonoraryChamberMenuDataContainer => TableRoot.Instance.GetDataContainer<HonoraryChamberMenuDataContainer>();

	public MonthlyHeroDataContainer MonthlyHeroDataContainer => TableRoot.Instance.GetDataContainer<MonthlyHeroDataContainer>();

	public NewbieRewardAttendanceDataContainer NewbieRewardAttendanceDataContainer => TableRoot.Instance.GetDataContainer<NewbieRewardAttendanceDataContainer>();

	public NewbieRewardTapOpenDataContainer NewbieTapOpenDataContainer => TableRoot.Instance.GetDataContainer<NewbieRewardTapOpenDataContainer>();

	public NewbieRewardProductDataContainer NewbieRewardProductDataContainer => TableRoot.Instance.GetDataContainer<NewbieRewardProductDataContainer>();

	public NewbieRewardProductMissionDataContainer NewbieRewardProductMissionDataContainer => TableRoot.Instance.GetDataContainer<NewbieRewardProductMissionDataContainer>();

	public NewbieRewardBannerUrlDataContainer NewbieRewardBannerUrlDataContainer => TableRoot.Instance.GetDataContainer<NewbieRewardBannerUrlDataContainer>();

	public DungeonHeroSetDataContainer DungeonHeroSetDataContainer => TableRoot.Instance.GetDataContainer<DungeonHeroSetDataContainer>();

	public GatewayToTrialDataContainer GatewayToTrialDataContainer => TableRoot.Instance.GetDataContainer<GatewayToTrialDataContainer>();

	public DungeonHeroListSetDataContainer DungeonHeroListSetDataContainer => TableRoot.Instance.GetDataContainer<DungeonHeroListSetDataContainer>();

	public GodkingTrialDataContainer GodkingTrialDataContainer => TableRoot.Instance.GetDataContainer<GodkingTrialDataContainer>();

	public GodkingTrialGroupDataContainer GodkingTrialGroupDataContainer => TableRoot.Instance.GetDataContainer<GodkingTrialGroupDataContainer>();

	public ItemBreakDataContainer ItemBreakDataContainer => TableRoot.Instance.GetDataContainer<ItemBreakDataContainer>();

	public SoulStoneRestoreDataContainer SoulStoneRestoreDataContainer => TableRoot.Instance.GetDataContainer<SoulStoneRestoreDataContainer>();

	public SoulWeaponOptionDataContainer SoulWeaponOptionDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponOptionDataContainer>();

	public SoulWeaponEtherDataContainer SoulWeaponEtherDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponEtherDataContainer>();

	public SoulWeaponReinforceDataContainer SoulWeaponReinforceDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponReinforceDataContainer>();

	public SoulWeaponUpgradeDataContainer SoulWeaponUpgradeDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponUpgradeDataContainer>();

	public SoulWeaponSkillDataContainer SoulWeaponSkillDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponSkillDataContainer>();

	public SoulWeaponEtherRateDataContainer SoulWeaponEtherRateDataContainer => TableRoot.Instance.GetDataContainer<SoulWeaponEtherRateDataContainer>();

	public EclipseStartDataContainer EclipseStartDataContainer => TableRoot.Instance.GetDataContainer<EclipseStartDataContainer>();

	public GameTitleSceneIntroduceDataContainer GameTitleSceneIntroduceDataContainer => TableRoot.Instance.GetDataContainer<GameTitleSceneIntroduceDataContainer>();

	public ClientCustomizingCameraDataContainer ClientCustomizingCameraDataContainer => TableRoot.Instance.GetDataContainer<ClientCustomizingCameraDataContainer>();

	public GuildArenaSeasonDataContainer GuildArenaSeasonDataContainer => TableRoot.Instance.GetDataContainer<GuildArenaSeasonDataContainer>();

	public GuildArenaTierDataContainer GuildArenaTierDataContainer => TableRoot.Instance.GetDataContainer<GuildArenaTierDataContainer>();

	public GuildArenaSeasonRewardDataContainer GuildArenaSeasonRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildArenaSeasonRewardDataContainer>();

	public GuildArenaServerBuffRewardDataContainer GuildArenaServerBuffRewardDataContainer => TableRoot.Instance.GetDataContainer<GuildArenaServerBuffRewardDataContainer>();

	public ChapterClearRewardDataContainer ChapterClearRewardDataContainer => TableRoot.Instance.GetDataContainer<ChapterClearRewardDataContainer>();

	public MainQuestDataContainer MainQuestDataContainer => TableRoot.Instance.GetDataContainer<MainQuestDataContainer>();

	public ContentsOpenConditionDataContainer ContentsOpenConditionDataContainer => TableRoot.Instance.GetDataContainer<ContentsOpenConditionDataContainer>();

	public EquipGachaClientDataContainer EquipGachaClientDataContainer => TableRoot.Instance.GetDataContainer<EquipGachaClientDataContainer>();

	public ItemEffectWidgetDataContainer ItemEffectWidgetDataContainer => TableRoot.Instance.GetDataContainer<ItemEffectWidgetDataContainer>();

	public PushRewardDataContainer PushRewardDataContainer => TableRoot.Instance.GetDataContainer<PushRewardDataContainer>();

	public AutoSeparationDataContainer AutoSeparationDataContainer => TableRoot.Instance.GetDataContainer<AutoSeparationDataContainer>();

	public ClearMissionProductDataContainer ClearMissionProductDataContainer => TableRoot.Instance.GetDataContainer<ClearMissionProductDataContainer>();

	public ClearMissionProductMissionDataContainer ClearMissionProductMissionDataContainer => TableRoot.Instance.GetDataContainer<ClearMissionProductMissionDataContainer>();

	public GetNewHeroSceneBundleDataContainer GetNewHeroSceneBundleDataContainer => TableRoot.Instance.GetDataContainer<GetNewHeroSceneBundleDataContainer>();

	public UnderPrisonDataContainer UnderPrisonDataContainer => TableRoot.Instance.GetDataContainer<UnderPrisonDataContainer>();

	public UnderPrisonDungeonDataContainer UnderPrisonDungeonDataContainer => TableRoot.Instance.GetDataContainer<UnderPrisonDungeonDataContainer>();

	public EquipAutoDataContainer EquipAutoDataContainer => TableRoot.Instance.GetDataContainer<EquipAutoDataContainer>();

	public ShakmehDungeonDataContainer ShakmehDungeonDataContainer => TableRoot.Instance.GetDataContainer<ShakmehDungeonDataContainer>();

	public ShakmehBossDataContainer ShakmehBossDataContainer => TableRoot.Instance.GetDataContainer<ShakmehBossDataContainer>();

	public CampaignDungeonSkillDataContainer CampaignDungeonSkillDataContainer => TableRoot.Instance.GetDataContainer<CampaignDungeonSkillDataContainer>();

	public DungeonPassiveEnvironmentDataContainer DungeonPassiveEnvironmentDataContainer => TableRoot.Instance.GetDataContainer<DungeonPassiveEnvironmentDataContainer>();

	public PurchaseDungeonDataContainer PurchaseDungeonDataContainer => TableRoot.Instance.GetDataContainer<PurchaseDungeonDataContainer>();

	public CinemaWindowDataContainer CinemaWindowDataContainer => TableRoot.Instance.GetDataContainer<CinemaWindowDataContainer>();

	public GachaSelectItemDataContainer GachaSelectItemDataContainer => TableRoot.Instance.GetDataContainer<GachaSelectItemDataContainer>();

	public HeroPresetStorageSlotDataContainer HeroPresetStorageSlotDataContainer => TableRoot.Instance.GetDataContainer<HeroPresetStorageSlotDataContainer>();

	public VideoDataContainer VideoDataContainer => TableRoot.Instance.GetDataContainer<VideoDataContainer>();

	public FrameControlDataContainer FrameControlDataContainer => TableRoot.Instance.GetDataContainer<FrameControlDataContainer>();

	public DownloadControlDataContainer DownloadControlDataContainer => TableRoot.Instance.GetDataContainer<DownloadControlDataContainer>();

	public PayShopDiscountDataContainer PayShopDiscountDataContainer => TableRoot.Instance.GetDataContainer<PayShopDiscountDataContainer>();

	public PayShopDiscountGroupDataContainer PayShopDiscountGroupDataContainer => TableRoot.Instance.GetDataContainer<PayShopDiscountGroupDataContainer>();

	public ValanceIdentifiedDataContainer ValanceIdentifiedDataContainer => TableRoot.Instance.GetDataContainer<ValanceIdentifiedDataContainer>();

	public ValanceFilterGroupDataContainer ValanceFilterGroupDataContainer => TableRoot.Instance.GetDataContainer<ValanceFilterGroupDataContainer>();

	public ValanceEnchantDataContainer ValanceEnchantDataContainer => TableRoot.Instance.GetDataContainer<ValanceEnchantDataContainer>();

	public ValanceEnchantOptionCountDataContainer ValanceEnchantOptionCountDataContainer => TableRoot.Instance.GetDataContainer<ValanceEnchantOptionCountDataContainer>();

	public ValanceCreatureFilterDataContainer ValanceCreatureFilterDataContainer => TableRoot.Instance.GetDataContainer<ValanceCreatureFilterDataContainer>();

	public OrdealArenaBuffDataContainer OrdealArenaBuffDataContainer => TableRoot.Instance.GetDataContainer<OrdealArenaBuffDataContainer>();

	public OrdealArenaEventDataContainer OrdealArenaEventDataContainer => TableRoot.Instance.GetDataContainer<OrdealArenaEventDataContainer>();

	public OrdealArenaNodeDataContainer OrdealArenaNodeDataContainer => TableRoot.Instance.GetDataContainer<OrdealArenaNodeDataContainer>();

	public OrdealArenaRewardAreaDataContainer OrdealArenaRewardAreaDataContainer => TableRoot.Instance.GetDataContainer<OrdealArenaRewardAreaDataContainer>();

	public OrdealArenaRewardRatingDataContainer OrdealArenaRewardRatingDataContainer => TableRoot.Instance.GetDataContainer<OrdealArenaRewardRatingDataContainer>();

	public OrdealArenaTierDataContainer OrdealArenaTierDataContainer => TableRoot.Instance.GetDataContainer<OrdealArenaTierDataContainer>();

	public EventRouletteDataContainer EventRouletteDataContainer => TableRoot.Instance.GetDataContainer<EventRouletteDataContainer>();

	public EventCraftDataContainer EventCraftDataContainer => TableRoot.Instance.GetDataContainer<EventCraftDataContainer>();

	public EventCraftEffectDataContainer EventCraftEffectDataContainer => TableRoot.Instance.GetDataContainer<EventCraftEffectDataContainer>();

	public EventForgeDataContainer EventForgeDataContainer => TableRoot.Instance.GetDataContainer<EventForgeDataContainer>();

	public KingsPassSeasonDataContainer KingsPassSeasonDataContainer => TableRoot.Instance.GetDataContainer<KingsPassSeasonDataContainer>();

	public KingsPassExpDataContainer KingsPassExpDataContainer => TableRoot.Instance.GetDataContainer<KingsPassExpDataContainer>();

	public KingsPassLevelDataContainer KingsPassLevelDataContainer => TableRoot.Instance.GetDataContainer<KingsPassLevelDataContainer>();

	public PunishmentGroupDataContainer PunishmentGroupDataContainer => TableRoot.Instance.GetDataContainer<PunishmentGroupDataContainer>();

	public PunishmentRaidDataContainer PunishmentRaidDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRaidDataContainer>();

	public PunishmentRaidTriggerDataContainer PunishmentRaidTriggerDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRaidTriggerDataContainer>();

	public PunishmentRaidRewardDataContainer PunishmentRaidRewardDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRaidRewardDataContainer>();

	public ShardItemDataContainer ShardItemDataContainer => TableRoot.Instance.GetDataContainer<ShardItemDataContainer>();

	public ShardFlaskItemDataContainer ShardFlaskItemDataContainer => TableRoot.Instance.GetDataContainer<ShardFlaskItemDataContainer>();

	public PetAwakenDataContainer PetAwakenDataContainer => TableRoot.Instance.GetDataContainer<PetAwakenDataContainer>();

	public PetUpgradeTierDataContainer PetUpgradeTierDataContainer => TableRoot.Instance.GetDataContainer<PetUpgradeTierDataContainer>();

	public PetOptionDataContainer PetOptionDataContainer => TableRoot.Instance.GetDataContainer<PetOptionDataContainer>();

	public JukeboxDataContainer JukeboxDataContainer => TableRoot.Instance.GetDataContainer<JukeboxDataContainer>();

	public PetBonusDataContainer PetBonusDataContainer => TableRoot.Instance.GetDataContainer<PetBonusDataContainer>();

	public PetTierInteractiveDataContainer PetTierInteractiveDataContainer => TableRoot.Instance.GetDataContainer<PetTierInteractiveDataContainer>();

	public PetCommonOptionOrderDataContainer PetCommonOptionOrderDataContainer => TableRoot.Instance.GetDataContainer<PetCommonOptionOrderDataContainer>();

	public PetIncubatorSlotDataContainer PetIncubatorSlotDataContainer => TableRoot.Instance.GetDataContainer<PetIncubatorSlotDataContainer>();

	public PetIncubatorDataContainer PetIncubatorDataContainer => TableRoot.Instance.GetDataContainer<PetIncubatorDataContainer>();

	public PetEggDataContainer PetEggDataContainer => TableRoot.Instance.GetDataContainer<PetEggDataContainer>();

	public TreasureHouseInfoDataContainer TreasureHouseInfoDataContainer => TableRoot.Instance.GetDataContainer<TreasureHouseInfoDataContainer>();

	public TreasureHouseDungeonDataContainer TreasureHouseDungeonDataContainer => TableRoot.Instance.GetDataContainer<TreasureHouseDungeonDataContainer>();

	public PetAdventureDataContainer PetAdventureDataContainer => TableRoot.Instance.GetDataContainer<PetAdventureDataContainer>();

	public PetAdventureRewardDataContainer PetAdventureRewardDataContainer => TableRoot.Instance.GetDataContainer<PetAdventureRewardDataContainer>();

	public GachaSelectShopDataContainer GachaSelectShopDataContainer => TableRoot.Instance.GetDataContainer<GachaSelectShopDataContainer>();

	public GachaSelectShopEffectDataContainer GachaSelectShopEffectDataContainer => TableRoot.Instance.GetDataContainer<GachaSelectShopEffectDataContainer>();

	public StaminaConsumePointDataContainer StaminaConsumePointDataContainer => TableRoot.Instance.GetDataContainer<StaminaConsumePointDataContainer>();

	public EventStepDataContainer EventStepDataContainer => TableRoot.Instance.GetDataContainer<EventStepDataContainer>();

	public KarmaCardDataContainer KarmaCardDataContainer => TableRoot.Instance.GetDataContainer<KarmaCardDataContainer>();

	public InfinityDataContainer InfinityDataContainer => TableRoot.Instance.GetDataContainer<InfinityDataContainer>();

	public InfinityRewardDataContainer InfinityRewardDataContainer => TableRoot.Instance.GetDataContainer<InfinityRewardDataContainer>();

	public InfinityRewardGroupDataContainer InfinityRewardGroupDataContainer => TableRoot.Instance.GetDataContainer<InfinityRewardGroupDataContainer>();

	public MissionCategoryDataContainer MissionCategoryDataContainer => TableRoot.Instance.GetDataContainer<MissionCategoryDataContainer>();

	public IconChatWindowDataContainer IconChatWindowDataContainer => TableRoot.Instance.GetDataContainer<IconChatWindowDataContainer>();

	public XCodeLocalizationDataContainer XCodeLocalizationDataContainer => TableRoot.Instance.GetDataContainer<XCodeLocalizationDataContainer>();

	public CommunityWebViewDataContainer CommunityWebViewDataContainer => TableRoot.Instance.GetDataContainer<CommunityWebViewDataContainer>();

	public LCSkillNameDataContainer LCSkillNameDataContainer => TableRoot.Instance.GetDataContainer<LCSkillNameDataContainer>();

	public PunishmentRuneDataContainer PunishmentRuneDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRuneDataContainer>();

	public PunishmentRuneOptionDataContainer PunishmentRuneOptionDataContainer => TableRoot.Instance.GetDataContainer<PunishmentRuneOptionDataContainer>();

	public KarmaDungeonDataContainer KarmaDungeonDataContainer => TableRoot.Instance.GetDataContainer<KarmaDungeonDataContainer>();

	public KarmaEnemyPowerUpDataContainer KarmaEnemyPowerUpDataContainer => TableRoot.Instance.GetDataContainer<KarmaEnemyPowerUpDataContainer>();

	public KarmaRewardDataContainer KarmaRewardDataContainer => TableRoot.Instance.GetDataContainer<KarmaRewardDataContainer>();

	public KarmaRewardExponentialDataContainer KarmaRewardExponentialDataContainer => TableRoot.Instance.GetDataContainer<KarmaRewardExponentialDataContainer>();

	public KarmaEnemyUnlockSkillDataContainer KarmaEnemyUnlockSkillDataContainer => TableRoot.Instance.GetDataContainer<KarmaEnemyUnlockSkillDataContainer>();

	public KarmaGaugeExponentialDataContainer KarmaGaugeExponentialDataContainer => TableRoot.Instance.GetDataContainer<KarmaGaugeExponentialDataContainer>();

	public NewPayShopProductDataContainer NewPayShopProductDataContainer => TableRoot.Instance.GetDataContainer<NewPayShopProductDataContainer>();

	public static void ResetClientEnvironment()
	{
		WavePlaying.DisableWinDecider = false;
		WavePlaying.ForceDecideWinner = false;
		WavePlaying.WinnerTeamType = TeamType.Left;
	}

	public GameRoot()
	{
		ArrayPool<string>.Alloc(1);
		ArrayPool<string>.Alloc(2);
		ArrayPool<string>.Alloc(3);
		ArrayPool<string>.Alloc(4);
		ArrayPool<string>.Alloc(5);
		JsonPool.CreateInstance();
		OperationRuntimeFactory = new OperationRuntimeFactory();
		OperationConditionFactory = new OperationConditionFactory();
		OperationConditionActionFactory = new OperationConditionActionFactory();
		OperationGroupConditionFactory = new OperationGroupConditionFactory();
		OperationModifierContainer = new OperationModifierContainer();
		StateActionFactory = new StateActionFactory();
		BattleLogFactory = new BattleLogFactory();
		TaskManager = new TaskManager();
		AiScriptContainer = new AiScriptContainer();
		WinDeciderFactory = new WinDeciderFactory();
		EventFactory = new EventFactory();
		ExponentialTableK = new ExponentialTable(1000L);
		ExponentialTableM = new ExponentialTable(1000000L);
		for (int i = 1; i <= 20; i++)
		{
			ExponentialTableK.GetAccRatio(100 * i, 3);
		}
	}

	public void InitForClient()
	{
		AsyncRun = delegate(Action runner, Action end)
		{
			SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
			{
				runner();
			}, end);
		};
		EventFactory.SetDeserializer(JsonMarshaler.Parse);
		EventFactory.SetSerializer(JsonMarshaler.EncodeJsonString);
	}

	public void OnDataContainerLoaded(bool editorVerify = true)
	{
		if (StateActionFactory != null)
		{
			StateActionFactory.Init(forceInitialize: true);
		}
	}

	private void VerifyTable(Action<string> onError)
	{
		foreach (EquipItemDetailData allDatum in EquipItemDetailDataContainer.GetAllData())
		{
			int maxStar = allDatum.MaxStar;
			int maxLevel = allDatum.MaxLevel;
			for (int i = 0; i <= maxStar; i++)
			{
				for (int j = 0; j <= maxLevel; j++)
				{
					if (EquipUpgradeStatDataContainer.GetData(i, allDatum.DetailIndex, j) == null)
					{
						onError("Missing equip upgrade data. star=" + i + " detailIndex=" + allDatum.DetailIndex + " level=" + j);
						return;
					}
				}
			}
		}
	}

	public void Update()
	{
		TaskManager.Flush();
	}

	private static GameRoot GetInstance()
	{
		if (_instance == null)
		{
			_instance = new GameRoot();
		}
		return _instance;
	}

	public void InitDataContainerAuto(TableUseProjectType projectType, IJsonLoader jsonLoader, Func<Type, IDictionary, object> parser)
	{
		TableDataAutoLoader.Load(this, projectType, jsonLoader, parser);
	}

	public void SetJsonLoader(IJsonLoader jsonLoader)
	{
		JsonLoader = jsonLoader;
	}

	public void SetAiScriptLoader(ITextLoader textSyncLoader)
	{
		AiScriptContainer.SetTextLoader(textSyncLoader);
	}

	public void UnitTestForClient()
	{
		SnapshotFactory.UnitTest();
	}

	public void UnitTestForServer()
	{
		SnapshotFactory.UnitTest();
	}
}
