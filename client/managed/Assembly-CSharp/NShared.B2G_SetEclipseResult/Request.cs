namespace NShared.B2G_SetEclipseResult;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_eclipse_result";

	public long AccountId { get; set; }

	public long[] FinishedAccountIds { get; set; }

	public int PlayTime { get; set; }

	public bool Win { get; set; }

	public bool IsMaster { get; set; }

	public int AvgHeroLevel { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public long TotalDamage { get; set; }

	public int MatchIndex { get; set; }

	public int CurrentDeckIndex { get; set; }

	public int CurrentChapterIndex { get; set; }

	public int CurrentDungeonIndex { get; set; }

	public int CurrentWaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int CurrentCreatureLevel { get; set; }

	public EclipseStatus LastStatus { get; set; }

	public string HeroConditionInfos { get; set; }

	public string KillMonsterInfos { get; set; }
}
