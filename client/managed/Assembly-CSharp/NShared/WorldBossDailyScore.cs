namespace NShared;

[JsonSerializable(Javascript = true)]
public class WorldBossDailyScore : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => WorldBossIndex;

	public int WorldBossIndex { get; set; }

	public int Season { get; set; }

	public long Score { get; set; }

	public int IsGetDailyParticipantReward { get; set; }

	public int GetDailyAchievementStep { get; set; }

	public string UpdateTime { get; set; }
}
