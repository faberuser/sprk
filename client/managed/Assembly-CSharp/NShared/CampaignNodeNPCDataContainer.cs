using NShared.NJit;

namespace NShared;

public class CampaignNodeNPCDataContainer : JitDataContainerDefault<ChapterDungeonKey2, CampaignNodeNPCData>
{
	public const string TABLENAME = "CampaignNodeNPCTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignNodeNPCTable");

	public CampaignNodeNPCData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}
}
