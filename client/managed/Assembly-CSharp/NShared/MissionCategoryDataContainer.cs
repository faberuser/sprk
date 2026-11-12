using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class MissionCategoryDataContainer : JitDataContainerDefault<MissionCategoryKey, MissionCategoryData>
{
	public const string TABLENAME = "MissionCategoryTable";

	private JitRef<List<MissionCategoryData>> _cachedMainMissionList = new JitRef<List<MissionCategoryData>>();

	private JitRef<List<MissionCategoryData>> _cachedWebEventList = new JitRef<List<MissionCategoryData>>();

	private JitRef<List<MissionCategoryData>> _cachedSpecialMissionList = new JitRef<List<MissionCategoryData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("MissionCategoryTable");

	public MissionCategoryDataContainer()
	{
		base.Packer.RegisterAndBind(_cachedMainMissionList);
		base.Packer.RegisterAndBind(_cachedWebEventList);
		base.Packer.RegisterAndBind(_cachedSpecialMissionList);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (MissionCategoryData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.IsMainMission())
			{
				_cachedMainMissionList.JsonLoad_GetOrNew().Add(item);
			}
			if (item.IsWebEvent())
			{
				_cachedWebEventList.JsonLoad_GetOrNew().Add(item);
			}
			if (item.IsAnySpecialMission())
			{
				_cachedSpecialMissionList.JsonLoad_GetOrNew().Add(item);
			}
		}
	}

	public MissionCategoryData GetData(int mainCategory, int subCategory)
	{
		return GetData(new MissionCategoryKey(mainCategory, subCategory));
	}

	public int GetSubCategoryCount(int mainCategory)
	{
		int num = 0;
		for (MissionCategoryData missionCategoryData = GetMissionCategoryDataHead(mainCategory); missionCategoryData != null; missionCategoryData = missionCategoryData.GetNextData())
		{
			num++;
		}
		return num;
	}

	public MissionCategoryData GetMissionCategoryDataHead(int mainCategory)
	{
		return GetData(new MissionCategoryKey(mainCategory, 1));
	}

	public List<MissionCategoryData> GetMissionCategoryList(int mainCategory)
	{
		MissionCategoryData missionCategoryData = GetMissionCategoryDataHead(mainCategory);
		List<MissionCategoryData> list = new List<MissionCategoryData>();
		while (missionCategoryData != null)
		{
			list.Add(missionCategoryData);
			missionCategoryData = missionCategoryData.GetNextData();
		}
		return list;
	}

	public List<MissionCategoryData> GetMainMissionList()
	{
		return new List<MissionCategoryData>(_cachedMainMissionList.Eval());
	}

	public List<MissionCategoryData> GetWebEventMissionList()
	{
		return new List<MissionCategoryData>(_cachedWebEventList.Eval());
	}

	public List<MissionCategoryData> GetSpecialMissionList()
	{
		return new List<MissionCategoryData>(_cachedSpecialMissionList.Eval());
	}
}
