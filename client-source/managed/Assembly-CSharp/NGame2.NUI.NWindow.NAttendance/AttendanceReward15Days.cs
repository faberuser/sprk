using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceReward15Days : AttendanceReward
{
	public class ComponentListner : AttendanceRewardComponent.Listener
	{
		public Action ClickerAction;

		public void OnClick(AttendanceRewardComponent component)
		{
			Vespa_System_Action_Extension_Method.fire(ClickerAction);
		}
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceReward15Days";

	private int _today;

	public AttendanceRewardComponent[] rewardComponents;

	protected override void SetupCustom()
	{
		base.SetupCustom();
		SetupReward();
	}

	private void SetupReward()
	{
		AttendanceRewards[] reward = base.AttendanceData.Reward;
		for (int i = 0; i < rewardComponents.Length; i++)
		{
			if (i < __Extension_Method_ICollection__.COUNT(reward))
			{
				AttendanceRewardInfoData data = GameRoot.Instance.AttendanceRewardInfoDataContainer.GetData(base.AttendanceData.Index, i + 1, 1);
				int num = i + 1;
				rewardComponents[i].Init(reward[i].Reward[0]);
				rewardComponents[i].SetListener(MakeListener(num));
				if (data != null)
				{
					rewardComponents[i].SetRewardInfo(data);
				}
				if (_today > num)
				{
					rewardComponents[i].SetAlreadyRewarded(rewarded: true);
				}
				else if (_today == num)
				{
					rewardComponents[i].SetAlreadyRewarded(rewarded: true);
					rewardComponents[i].SetTodayReward(today: true);
				}
				else
				{
					rewardComponents[i].SetTodayReward(today: false);
					rewardComponents[i].SetAlreadyRewarded(rewarded: false);
				}
			}
		}
	}

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.Prefab15DaysDetail;
	}

	protected override List<NShared.AttendanceReward> GetRewardDataList()
	{
		List<NShared.AttendanceReward> list = new List<NShared.AttendanceReward>();
		if (base.AttendanceData == null)
		{
			return list;
		}
		if (base.AttendanceInfo == null)
		{
			return list;
		}
		int completedDay = GetCompletedDay();
		for (int i = 0; i < base.AttendanceData.Reward.Count(); i++)
		{
			AttendanceRewards attendanceRewards = base.AttendanceData.Reward[i];
			for (int j = 0; j < attendanceRewards.Reward.Length; j++)
			{
				int day = attendanceRewards.Reward[j].Day;
				if ((base.RewardableDay > 0) ? (day == base.RewardableDay) : (day == completedDay))
				{
					list.Add(attendanceRewards.Reward[j]);
					_today = day;
				}
			}
		}
		RewardSlotCount = list.Count;
		return list;
	}

	protected override void OnCreateComponent(int index, AttendanceRewardComponent component)
	{
		component.SetListener(this);
		component.Show(show: true);
		AttendanceRewardInfoData data = GameRoot.Instance.AttendanceRewardInfoDataContainer.GetData(base.AttendanceData.Index, _today, index + 1);
		if (data != null)
		{
			component.SetRewardInfo(data);
		}
	}

	public List<NShared.AttendanceReward> SearchRewards(int searchDay)
	{
		List<NShared.AttendanceReward> list = new List<NShared.AttendanceReward>();
		for (int i = 0; i < base.AttendanceData.Reward.Count(); i++)
		{
			AttendanceRewards attendanceRewards = base.AttendanceData.Reward[i];
			for (int j = 0; j < attendanceRewards.Reward.Length; j++)
			{
				int day = attendanceRewards.Reward[j].Day;
				if (day == searchDay)
				{
					list.Add(attendanceRewards.Reward[j]);
					_today = day;
				}
			}
		}
		return list;
	}

	public AttendanceRewardComponent.Listener MakeListener(int day)
	{
		return new ComponentListner
		{
			ClickerAction = delegate
			{
				FactoryBase.ReleaseAndClearList(_rewardList);
				List<NShared.AttendanceReward> list = SearchRewards(day);
				HashSet<NShared.AttendanceReward> hashSet = new HashSet<NShared.AttendanceReward>();
				foreach (NShared.AttendanceReward item in list)
				{
					if (item.Invisible)
					{
						hashSet.Add(item);
					}
				}
				list.RemoveAll(hashSet.Contains);
				List<DbRewardData> list2 = new List<DbRewardData>(list.ToArray());
				Init(list2, GetRewardComponentPrefabAuto(), GetSpecialRewardComponentPrefabAuto(), list2.Count, OnCreateComponent);
			}
		};
	}
}
