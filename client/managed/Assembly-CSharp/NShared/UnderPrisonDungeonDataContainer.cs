using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class UnderPrisonDungeonDataContainer : JitDataContainerDefault<UnderPrisonDungeonKey, UnderPrisonDungeonData>
{
	public const string TABLENAME = "UnderPrisonDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("UnderPrisonDungeonTable");

	public List<UnderPrisonDungeonData> GetUnderPrisonDungeonDataListByChpaterIndex(int chapterIndex)
	{
		IList<UnderPrisonDungeonData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		List<UnderPrisonDungeonData> list = new List<UnderPrisonDungeonData>();
		list.Clear();
		UnderPrisonDungeonData underPrisonDungeonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			underPrisonDungeonData = allData[i];
			if (underPrisonDungeonData != null && underPrisonDungeonData.ChapterIndex == chapterIndex)
			{
				list.Add(underPrisonDungeonData);
			}
		}
		return list;
	}

	public UnderPrisonDungeonData GetUnderPrisonDungeonData(int chapterIndex, int dungeonIndex)
	{
		IList<UnderPrisonDungeonData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		UnderPrisonDungeonData underPrisonDungeonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			underPrisonDungeonData = allData[i];
			if (underPrisonDungeonData != null && underPrisonDungeonData.ChapterIndex == chapterIndex && underPrisonDungeonData.DungeonIndex == dungeonIndex)
			{
				return underPrisonDungeonData;
			}
		}
		return null;
	}

	public int GetLastDiffByChapterIndex(int chapterIndex)
	{
		IList<UnderPrisonDungeonData> allData = GetAllData();
		if (allData == null)
		{
			return 0;
		}
		UnderPrisonDungeonData underPrisonDungeonData = null;
		int num = 0;
		for (int i = 0; i < allData.Count; i++)
		{
			underPrisonDungeonData = allData[i];
			if (underPrisonDungeonData != null && underPrisonDungeonData.ChapterIndex == chapterIndex && num < underPrisonDungeonData.Diff)
			{
				num = underPrisonDungeonData.Diff;
			}
		}
		return num;
	}
}
