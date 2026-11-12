using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PartyDungeonDataContainer : JitDataContainerDefault<ChapterDungeonKey2, PartyDungeonData>
{
	public const string TABLENAME = "PartyDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PartyDungeonTable");

	public PartyDungeonData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public List<PartyDungeonData> GetFilteredData(PartyDungeonType requireDungeonType, PartyDungeonChapter requireChapterType, PartyDungeonDifficultyType reqDiff, int requireDungeonIndex, Func<PartyDungeonData, bool> predicate = null)
	{
		List<PartyDungeonData> list = new List<PartyDungeonData>();
		foreach (PartyDungeonData allDatum in GetAllData())
		{
			if (allDatum.DungeonType != PartyDungeonType.None && (requireDungeonType == PartyDungeonType.None || allDatum.DungeonType == requireDungeonType) && (requireChapterType == PartyDungeonChapter.None || allDatum.Chapter == requireChapterType) && (requireDungeonIndex == 0 || allDatum.DungeonIndex == requireDungeonIndex) && (predicate == null || predicate(allDatum)))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}
}
