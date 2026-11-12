using NShared.NJit;

namespace NShared;

public class WorldBossDataContainer : JitDataContainerDefault<int, WorldBossData>
{
	public const string TABLENAME = "WorldBossTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossTable");

	public bool IsWorldBossChapter(int chapterIndex)
	{
		return GetData(chapterIndex) != null;
	}
}
