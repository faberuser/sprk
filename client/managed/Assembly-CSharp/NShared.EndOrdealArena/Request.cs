namespace NShared.EndOrdealArena;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ordeal_arena/end_ordeal_arena";

	public int NodeIndex { get; set; }

	public int[] HeroIndices { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public bool Completed { get; set; }

	public string BattleLogString { get; set; }

	public string CreatureInfoString { get; set; }

	public int PureBattleTime { get; set; }

	public long ElapsedBattleTime { get; set; }
}
