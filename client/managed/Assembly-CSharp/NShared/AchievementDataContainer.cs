using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class AchievementDataContainer : JitDataContainerDefault<AchievementKey2, AchievementData>
{
	public const string TABLENAME = "AchievementTable";

	private JitDictionary<AchievementType, HashSet<int>> _achievementIndices_by_Type = new JitDictionary<AchievementType, HashSet<int>>();

	private JitDictionary<int, HashSet<int>> _achievementIndices_by_Hero = new JitDictionary<int, HashSet<int>>();

	private JitRef<HashSet<int>> _allAchievementIndices_of_Hero = new JitRef<HashSet<int>>();

	private JitDictionary<string, HashSet<int>> _achievementIndices_by_Event = new JitDictionary<string, HashSet<int>>();

	private JitDictionary<int, int> _achievements_by_HourOfDay = new JitDictionary<int, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("AchievementTable");

	public AchievementDataContainer()
	{
		base.Packer.RegisterAndBind(_achievementIndices_by_Type);
		base.Packer.RegisterAndBind(_achievementIndices_by_Event);
		base.Packer.RegisterAndBind(_achievements_by_HourOfDay);
		base.Packer.RegisterAndBind(_achievementIndices_by_Hero);
		base.Packer.RegisterAndBind(_allAchievementIndices_of_Hero);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (AchievementData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_achievementIndices_by_Type.JsonLoad_AddToHashSetValue(item.Type, item.Index);
			if (item.Type == AchievementType.Hero && item.ReqType == AchievementReqType.HeroIndex)
			{
				int result = 0;
				int.TryParse(item.ReqValue, out result);
				if (result > 0)
				{
					_achievementIndices_by_Hero.JsonLoad_AddToHashSetValue(result, item.Index);
					_allAchievementIndices_of_Hero.JsonLoad_GetOrNew().Add(item.Index);
				}
			}
			if (!string.IsNullOrEmpty(item.EventName))
			{
				_achievementIndices_by_Event.JsonLoad_AddToHashSetValue(item.EventName, item.Index);
			}
		}
		BuildHourAchievementDictionary_in_JsonLoad();
	}

	private void BuildHourAchievementDictionary_in_JsonLoad()
	{
		List<AchievementData> list = (from v in base.BaseList.JsonLoad_GetOrNew()
			where v.ReqType == AchievementReqType.LoginOrConnectedTime
			select v).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 23;
		int hour;
		for (hour = num; hour <= num2; hour++)
		{
			AchievementData achievementData = list.Find((AchievementData data) => data.ReqSub1Value <= hour && data.ReqSub2Value > hour);
			if (achievementData != null)
			{
				_achievements_by_HourOfDay.JsonLoad_Get().Add(hour, achievementData.Index);
				continue;
			}
			int nexthour;
			for (nexthour = hour; nexthour <= num2; nexthour++)
			{
				AchievementData achievementData2 = list.Find((AchievementData data) => data.ReqSub1Value == nexthour);
				if (achievementData2 == null)
				{
					_achievements_by_HourOfDay.JsonLoad_Get().Add(hour, achievementData2.Index);
					break;
				}
			}
		}
	}

	public AchievementData GetData(int achievementIndex, int step)
	{
		return GetData(new AchievementKey2(achievementIndex, step));
	}

	public HashSet<int> GetAchievementIndexSet(AchievementType achievementType)
	{
		_achievementIndices_by_Type.TryGetValue(achievementType, out var value);
		return value;
	}

	public HashSet<int> GetHeroAchievementIndexSet(int HeroIndex)
	{
		_achievementIndices_by_Hero.TryGetValue(HeroIndex, out var value);
		return value;
	}

	public HashSet<int> GetAllHeroAchievementIndices()
	{
		return _allAchievementIndices_of_Hero.Eval();
	}

	public HashSet<int> GetAchievementIndexSet(string eventName)
	{
		_achievementIndices_by_Event.TryGetValue(eventName, out var value);
		return value;
	}

	public bool CheckAchievementType(int achievementIndex, AchievementType achievementType)
	{
		if (_achievementIndices_by_Type.TryGetValue(achievementType, out var value) && value.Contains(achievementIndex))
		{
			return true;
		}
		return false;
	}

	public List<AchievementData> SelectAchievementOfType(AchievementType achievementType, Func<AchievementData, bool> selector)
	{
		List<AchievementData> list = new List<AchievementData>();
		if (selector == null)
		{
			return list;
		}
		HashSet<int> achievementIndexSet = GetAchievementIndexSet(achievementType);
		if (achievementIndexSet == null || achievementIndexSet.Count == 0)
		{
			return list;
		}
		foreach (int item in achievementIndexSet)
		{
			int num = 1;
			while (true)
			{
				AchievementData data = GetData(item, num);
				if (data == null)
				{
					break;
				}
				if (selector(data))
				{
					list.Add(data);
				}
				num++;
			}
		}
		return list;
	}

	public int GetLoginOrConnectedAchievementIndex(int hourOfDay)
	{
		return _achievements_by_HourOfDay.GetOrDefault(hourOfDay, 0);
	}
}
