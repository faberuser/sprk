using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardNew14Days : AttendanceReward
{
	public class ComponentListner : AttendanceRewardComponent.Listener
	{
		public Action<AttendanceRewardComponent> ClickerAction;

		public void OnClick(AttendanceRewardComponent component)
		{
			Vespa_System_Action_Extension_Method.fire(ClickerAction, component);
		}
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardNew14Days";

	[Header("[[ --- AttendanceRewardNew14Days ]]")]
	public GameObject GoPageDefault;

	public GameObject GoPageOther;

	public UILabel Label_RemainTime;

	private int _pageNum = -1;

	private float _lastUpdateTick_of_RemainTime = -9999f;

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabNew14DaysAuto;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastUpdateTick_of_RemainTime > 1f)
		{
			_lastUpdateTick_of_RemainTime = Time.time;
			string remainTimeString = Attendance.GetRemainTimeString(base.AttendanceInfo);
			if (String_Extension_Method.IsNullOrEmpty(remainTimeString))
			{
				UIHelper.EnableGameObject(Label_RemainTime, enable: false);
				UIHelper.SetLabelText(Label_RemainTime, string.Empty);
			}
			else
			{
				UIHelper.EnableGameObject(Label_RemainTime, enable: true);
				UIHelper.SetLabelText(Label_RemainTime, remainTimeString);
			}
		}
	}

	public override int GetCurrentPage()
	{
		if (_pageNum == -1)
		{
			_pageNum = base.GetCurrentPage();
		}
		return _pageNum;
	}

	public bool IsValidPage(int pageNum)
	{
		if (base.AttendanceData == null || base.AttendanceData.Reward == null)
		{
			return false;
		}
		int pageSize = GetPageSize();
		int num = pageNum * pageSize;
		if (__Extension_Method_ICollection__.COUNT(base.AttendanceData.Reward) <= num)
		{
			return false;
		}
		return true;
	}

	public int GetNextPage()
	{
		int currentPage = GetCurrentPage();
		if (!IsValidPage(currentPage + 1))
		{
			if (currentPage != 0)
			{
				_pageNum = 0;
			}
		}
		else
		{
			_pageNum++;
		}
		return GetCurrentPage();
	}

	public override void Reset()
	{
		base.Reset();
		_pageNum = -1;
	}

	public void OnClickNextPage()
	{
		int nextPage = GetNextPage();
		FactoryBase.ReleaseAndClearList(_rewardList);
		SetPage(nextPage);
		List<NShared.AttendanceReward> rewardDataList = GetRewardDataList();
		HashSet<NShared.AttendanceReward> hashSet = new HashSet<NShared.AttendanceReward>();
		foreach (NShared.AttendanceReward item in rewardDataList)
		{
			if (item != null && item.Invisible)
			{
				hashSet.Add(item);
			}
		}
		rewardDataList.RemoveAll(hashSet.Contains);
		List<DbRewardData> list = new List<DbRewardData>(rewardDataList.ToArray());
		_todayComponent = null;
		Init(list, GetRewardComponentPrefabAuto(), GetSpecialRewardComponentPrefabAuto(), list.Count, OnCreateComponent);
		ResetComponents();
	}

	private void ResetComponents()
	{
		GetCurrentPage();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardList); i++)
		{
			AttendanceRewardComponent attendanceRewardComponent = _rewardList[i] as AttendanceRewardComponent;
			if (!(attendanceRewardComponent == null))
			{
				GetComponentDay(attendanceRewardComponent);
				attendanceRewardComponent.SetSpecial(isOn: false);
			}
		}
	}

	public void SetPage(int page)
	{
		bool flag = page == 0;
		UIHelper.EnableGameObject(GoPageDefault, flag);
		UIHelper.EnableGameObject(GoPageOther, !flag);
	}

	protected override void SetupCustom()
	{
		base.SetupCustom();
		ResetComponents();
		int currentPage = GetCurrentPage();
		SetPage(currentPage);
	}
}
