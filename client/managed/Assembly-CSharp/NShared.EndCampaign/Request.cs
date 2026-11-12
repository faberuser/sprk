namespace NShared.EndCampaign;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/end_campaign";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public bool ScenarioDungeon { get; set; }

	public bool Completed { get; set; }

	public int Star { get; set; }

	public int[] MonsterIndices { get; set; }

	public int[] KillCounts { get; set; }

	public int[] AliveHeroIndices { get; set; }

	public string BattleLogString { get; set; }

	public string CreatureInfoString { get; set; }

	public int PureBattleTime { get; set; }

	public long ElapsedBattleTime { get; set; }

	public long TotalDamage { get; set; }
}
