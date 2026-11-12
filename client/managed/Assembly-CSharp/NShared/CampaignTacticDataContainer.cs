using NShared.NJit;

namespace NShared;

public class CampaignTacticDataContainer : JitDataContainerDefault<CampaignTacticKey, CampaignTacticData>, ISnapshotMemberSerializer
{
	public const string TABLENAME = "CampaignTacticTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignTacticTable");

	public CampaignTacticData GetData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		return GetData(new CampaignTacticKey(chapterIndex, dungeonIndex, difficulty));
	}

	public CampaignTacticData GetData(int chapterIndex, int dungeonIndex, int difficulty)
	{
		return GetData(new CampaignTacticKey(chapterIndex, dungeonIndex, (DungeonDifficultyType)difficulty));
	}

	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		if (value == null)
		{
			return null;
		}
		string[] array = value.Split(',');
		if (__Extension_Method_ICollection__.COUNT(array) < 3)
		{
			return null;
		}
		if (!int.TryParse(array[0], out var result))
		{
			return null;
		}
		if (!int.TryParse(array[1], out var result2))
		{
			return null;
		}
		if (!int.TryParse(array[2], out var result3))
		{
			return null;
		}
		return GetData(result, result2, result3);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (!(value is CampaignTacticData campaignTacticData))
		{
			return null;
		}
		CampaignTacticKey kEY = campaignTacticData.GETKEY();
		return $"{kEY.ChapterIndex},{kEY.DungeonIndex},{(int)kEY.Difficulty}";
	}
}
