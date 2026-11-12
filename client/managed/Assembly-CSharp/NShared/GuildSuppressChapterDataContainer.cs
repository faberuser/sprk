using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GuildSuppressChapterDataContainer : JitDataContainerDefault<int, GuildSuppressChapterData>
{
	public const string TABLENAME = "GuildSuppressChapterTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressChapterTable");

	public int[] GetAllChapterIndices()
	{
		List<int> list = new List<int>();
		foreach (GuildSuppressChapterData allDatum in GetAllData())
		{
			list.Add(allDatum.RaidIndex);
		}
		return list.ToArray();
	}

	public bool IsGuildSuppressChpater(int chapterIndex)
	{
		return GetData(chapterIndex) != null;
	}
}
