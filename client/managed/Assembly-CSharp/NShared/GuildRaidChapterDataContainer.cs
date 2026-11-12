using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GuildRaidChapterDataContainer : JitDataContainerDefault<int, GuildRaidChapterData>
{
	public const string TABLENAME = "GuildRaidChapterTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildRaidChapterTable");

	public int[] GetAllChapterIndices()
	{
		List<int> list = new List<int>();
		foreach (GuildRaidChapterData allDatum in GetAllData())
		{
			list.Add(allDatum.ChapterIndex);
		}
		return list.ToArray();
	}

	public bool IsGuildRaidChpater(int chapterIndex)
	{
		return GetData(chapterIndex) != null;
	}

	public bool CheckIsBonusDungeon(int chapterIndex)
	{
		return GetData(chapterIndex)?.IsBonusDungeon ?? false;
	}
}
