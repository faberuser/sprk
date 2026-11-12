using NShared.NJit;

namespace NShared;

public class DungeonHeroSetDataContainer : JitDataContainerDefault<DungeonHeroSetKey, DungeonHeroSetData>
{
	public const string TABLENAME = "DungeonHeroSetTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DungeonHeroSetTable");

	public bool IsDungeonHeroSet(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		return GetData(new DungeonHeroSetKey(chapterIndex, dungeonIndex, difficulty)) != null;
	}

	public DungeonHeroSetData GetData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		return GetData(new DungeonHeroSetKey(chapterIndex, dungeonIndex, difficulty));
	}
}
