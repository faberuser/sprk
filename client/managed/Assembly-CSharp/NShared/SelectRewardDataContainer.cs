using NShared.NJit;

namespace NShared;

public class SelectRewardDataContainer : JitDataContainerDefault<ChapterDungeonKey2, SelectRewardData>
{
	public const string TABLENAME = "SelectRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SelectRewardTable");

	public SelectRewardData GetData(int ChapterIndex, int DungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(ChapterIndex, DungeonIndex));
	}
}
