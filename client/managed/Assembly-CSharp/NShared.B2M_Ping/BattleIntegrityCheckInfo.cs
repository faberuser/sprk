namespace NShared.B2M_Ping;

[JsonSerializable(Javascript = true)]
public class BattleIntegrityCheckInfo
{
	public string RequestServer { get; set; }

	public IntegrityType IntegrityType { get; set; }

	public string RequestId { get; set; }

	public long AccountId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int Difficulty { get; set; }

	public BattleType BattleType { get; set; }

	public string BattleLogString { get; set; }

	public BattleInstanceExtraData ExtraData { get; set; }

	public CachedHeroInfo[] LeftHeroInfos { get; set; }

	public ResultCreatureInfo[] LeftInitialHeroInfos { get; set; }

	public CachedHeroInfo[] RightHeroInfos { get; set; }

	public ResultCreatureInfo[] RightInitialHeroInfos { get; set; }

	public string CreatureInfoString { get; set; }

	public CheckPriority CheckPriority { get; set; }

	public BattleIntegrityCheckMode CheckMode { get; set; }
}
