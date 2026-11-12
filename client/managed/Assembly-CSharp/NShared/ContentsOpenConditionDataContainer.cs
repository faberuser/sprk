using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ContentsOpenConditionDataContainer : JitDataContainerDefault<int, ContentsOpenConditionData>
{
	public const string TABLENAME = "ContentsOpenConditionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ContentsOpenConditionTable");

	public ContentsOpenConditionData GetDataByContentsType(ContentsType contentsType)
	{
		IList<ContentsOpenConditionData> allData = GetAllData();
		ContentsOpenConditionData contentsOpenConditionData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			contentsOpenConditionData = allData[i];
			if (contentsOpenConditionData != null && contentsType == contentsOpenConditionData.ContentsType)
			{
				return contentsOpenConditionData;
			}
		}
		return null;
	}

	public int GetSubQuestIndexByNode(int chapterIndex, int dungeonIndex)
	{
		ContentsOpenConditionData contentsOpenConditionData = null;
		for (int i = 0; i < GetAllData().Count; i++)
		{
			contentsOpenConditionData = GetAllData()[i];
			if (contentsOpenConditionData != null && contentsOpenConditionData.ContentsType == ContentsType.Node && contentsOpenConditionData.ConditionValue != null && contentsOpenConditionData.ConditionValue.Length > 1 && contentsOpenConditionData.ConditionValue[0] == chapterIndex && contentsOpenConditionData.ConditionValue[1] == dungeonIndex)
			{
				return contentsOpenConditionData.OpenConditionSubQuest;
			}
		}
		return 0;
	}

	public List<ContentsOpenConditionData> GetOpenContentsListBySubQuestIndex(int index)
	{
		IList<ContentsOpenConditionData> allData = GetAllData();
		List<ContentsOpenConditionData> list = new List<ContentsOpenConditionData>();
		list.Clear();
		ContentsOpenConditionData contentsOpenConditionData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			contentsOpenConditionData = allData[i];
			if (contentsOpenConditionData != null && contentsOpenConditionData.OpenConditionSubQuest == index)
			{
				list.Add(contentsOpenConditionData);
			}
		}
		return list;
	}
}
