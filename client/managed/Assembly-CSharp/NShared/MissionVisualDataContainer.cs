using System.Collections.Generic;
using NShared.NJit;
using NVespa;

namespace NShared;

public class MissionVisualDataContainer : JitDataContainerDefault<MissionVisualKey, MissionVisualData>
{
	public const string TABLENAME = "MissionVisualTable";

	private JitDictionary<int, MissionVisualData> _dicKeyByQuestIndex = new JitDictionary<int, MissionVisualData>();

	public override TablePathDefine PathDefine => new TablePathDefine("MissionVisualTable");

	public MissionVisualDataContainer()
	{
		base.Packer.RegisterAndBind(_dicKeyByQuestIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (MissionVisualData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.QuestIndex != 0)
			{
				if (_dicKeyByQuestIndex.JsonLoad_Get().ContainsKey(item.QuestIndex))
				{
					VespaDebug.LogError("[MissionVisualData] QuestIndex Duplicate : " + item.QuestIndex);
					break;
				}
				_dicKeyByQuestIndex.JsonLoad_Get().Add(item.QuestIndex, item);
			}
		}
	}

	public MissionVisualData GetMissionVisualDataHead(int mainCategory, int subCategory)
	{
		MissionVisualKey key = new MissionVisualKey(mainCategory, subCategory, 1, 0);
		return GetData(key);
	}

	public List<MissionVisualData> GetMissionVisualDataList(int mainCategory, int subCategory)
	{
		MissionVisualData missionVisualData = GetMissionVisualDataHead(mainCategory, subCategory);
		List<MissionVisualData> list = new List<MissionVisualData>();
		while (missionVisualData != null)
		{
			list.Add(missionVisualData);
			missionVisualData = missionVisualData.GetNextData();
		}
		return list;
	}

	public MissionVisualData GetMissionVisualDataByQuestIndex(int questIndex)
	{
		return _dicKeyByQuestIndex.GetOrDefault(questIndex);
	}

	public MissionVisualData GetNextVisualData(int questIndex)
	{
		MissionVisualData missionVisualData = GetMissionVisualDataByQuestIndex(questIndex);
		if (missionVisualData != null)
		{
			missionVisualData = missionVisualData.GetNextSubStepData();
		}
		return missionVisualData;
	}
}
