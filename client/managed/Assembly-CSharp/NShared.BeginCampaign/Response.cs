using NShared.NWorldBoss;

namespace NShared.BeginCampaign;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RewardItemInfo[] DropItems { get; set; }

	public int DropGold { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public GemResultInfo GemResult { get; set; }

	public CurrencyResultInfo[] CurrencyResults { get; set; }

	public string RechargeTime { get; set; }

	public string BattleServerAddress { get; set; }

	public int BattleServerPort { get; set; }

	public bool CheckIntegrity { get; set; }

	public ResultCreatureInfo[] TowerIntialUserHeroes { get; set; }

	public ResultCreatureInfo[] TowerIntialEnemyHeroes { get; set; }

	public WorldBossInfo WorldBossInfo { get; set; }

	public WorldBossDailyScore WorldBossDailyScore { get; set; }

	public EventWorldBossInfo EventWorldBossInfo { get; set; }

	public GodkingTrialDungeonInfo GodkingTrialDungeonInfo { get; set; }

	public ItemBoosterResultInfo ItemBoosterResult { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }

	public GuildSuppressInfo GuildSuppressInfo { get; set; }

	public GuildSuppressScoreInfo GuildSuppressScoreInfo { get; set; }

	public GuildSuppressPlayInfo GuildSuppressPlayInfo { get; set; }

	public int WearCostumeCount { get; set; }

	public GuildRaidDungeonInfo GuildRaidDungeonInfo { get; set; }

	public EclipseBattleInfo EclipseBattleInfo { get; set; }
}
