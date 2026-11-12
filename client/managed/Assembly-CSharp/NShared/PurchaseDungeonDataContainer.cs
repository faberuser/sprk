using NShared.NJit;

namespace NShared;

public class PurchaseDungeonDataContainer : JitDataContainerDefault<ChapterDungeonKey2, PurchaseDungeonData>
{
	public const string TABLENAME = "PurchaseDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PurchaseDungeonTable");

	public PurchaseDungeonData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}
}
