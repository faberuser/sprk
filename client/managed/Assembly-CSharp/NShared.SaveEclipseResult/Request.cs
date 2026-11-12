namespace NShared.SaveEclipseResult;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "eclipse/save_eclipse_result";

	public long AccountId { get; set; }

	public int PlayTime { get; set; }

	public long TotalDamage { get; set; }

	public int MatchIndex { get; set; }

	public int CurrentDeckIndex { get; set; }

	public int CurrentChapterIndex { get; set; }

	public int CurrentDungeonIndex { get; set; }

	public int CurrentWaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int CurrentCreatureLevel { get; set; }

	public EclipseStatus LastStatus { get; set; }

	public string BattleLogString { get; set; }

	public string CreatureInfoString { get; set; }
}
