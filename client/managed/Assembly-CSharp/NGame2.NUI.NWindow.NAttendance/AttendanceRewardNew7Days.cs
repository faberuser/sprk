using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardNew7Days : AttendanceReward
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardNew7Days";

	[Header("[[ --- AttendanceRewardNew7Days ]]")]
	public UILabel Label_RemainTime;

	private float _lastUpdateTick_of_RemainTime = -9999f;

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabNew7DaysAuto;
	}

	protected override PrefabAuto<AttendanceRewardComponent> GetSpecialRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabNew7DaysSpecialAuto;
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
}
