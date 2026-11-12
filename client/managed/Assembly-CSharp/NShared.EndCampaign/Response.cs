using NShared.NRaid;
using NShared.NWorldBoss;

namespace NShared.EndCampaign;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string ForwardHost { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ChapterDungeonInfo[] CampaignResults { get; set; }

	public ExpResultInfo ExpResult { get; set; }

	public ExpResultInfo[] ExpResultsByGetHero { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos { get; set; }

	public HeroExpResultInfo[] HeroExpResults { get; set; }

	public HeroInfo[] HeroInfos { get; set; }

	public TowerInfo[] TowerInfos { get; set; }

	public ResultCreatureInfo[] TowerHeroInfos { get; set; }

	public StaminaResultInfo UndergroundPrisonKeyResult { get; set; }

	public FlaskEndCampaignResultInfo[] FlaskResults { get; set; }

	public ItemResultInfo[] FlaskItemResults { get; set; }

	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public ExpResultInfo ExpResult2 { get; set; }

	public HeroInfo[] HeroInfos2 { get; set; }

	public StaminaResultInfo StaminaResult2 { get; set; }

	public ItemResultInfo[] ItemResults2 { get; set; }

	public EquipItemInfo[] EquipItemInfos2 { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos2 { get; set; }

	public HideoutDungeonInfo HideoutDungeonResult { get; set; }

	public ItemBoosterResultInfo ItemBoosterResult { get; set; }

	public int DailyCompletedCount { get; set; }

	public int Diff { get; set; }

	public WorldMapEventTimeInfo[] WorldMapEventTimeInfos { get; set; }

	public WorldMapEventInfo[] WorldMapEventInfos { get; set; }

	public WorldMapEventInfo[] RemovedWorldMapEventInfos { get; set; }

	public RaidInfo CompletedRaidInfo { get; set; }

	public RaidResultInfo RaidResult { get; set; }

	public long[] MultiplayMemberIds { get; set; }

	public bool MultiplayRewardCalculated { get; set; }

	public int MultiplayRemainTimeoutTime { get; set; }

	public RewardItemInfo[] MultiplayRewardItems { get; set; }

	public MultiplayMemberResultInfo[] MultiplayMemberResults { get; set; }

	public GuildPointResultInfo GuildPointResult { get; set; }

	public GuildActivityPointResultInfo GuildActivityPointResult { get; set; }

	public GuildMemberAccActivityPointResultInfo GuildMemberAccActivityPointResult { get; set; }

	public CurrencyResultInfo3[] GuildCurrencyResults { get; set; }

	public GuildRaidDungeonInfo[] GuildRaidInfos { get; set; }

	public GuildRaidMemberTotalScoreInfo GuildRaidMemberTotalScoreInfo { get; set; }

	public IndexedRewardItemInfo[] GuildRaidBootyItemRewardInfos { get; set; }

	public WorldBossInfo WorldBossInfo { get; set; }

	public WorldBossRankerInfo WorldBossRankInfo { get; set; }

	public bool WorldBossKilledByMe { get; set; }

	public PolledRewardInfo WorldBossKilledRewardInfo { get; set; }

	public EventWorldBossInfo EventWorldBossInfo { get; set; }

	public bool EventWorldBossKilledByMe { get; set; }

	public EventWorldBossScoreInfo EventWorldBossScoreInfo { get; set; }

	public ChallengeRaidRankerInfo ChallengeRaidRankerInfo { get; set; }

	public GuildSuppressInfo GuildSuppressInfo { get; set; }

	public GuildSuppressScoreInfo GuildSuppressScoreInfo { get; set; }

	public GuildSuppressPlayInfo GuildSuppressPlayInfo { get; set; }

	public int WearCostumeCount { get; set; }

	public ConquestDungeonInfo ConquestDungeonResult { get; set; }

	public GodkingTrialDungeonInfo GodkingTrialDungeonInfo { get; set; }

	public StaminaResultInfo EclipseStaminaResult { get; set; }

	public EclipseDungeonInfo EclipseDungeonInfo { get; set; }

	public UnderPrisonInfo[] UnderPrisonInfos { get; set; }

	public string[] SelectedRewardItemCodes { get; set; }
}
