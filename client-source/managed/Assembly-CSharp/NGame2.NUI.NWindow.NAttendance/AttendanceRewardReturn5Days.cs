using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardReturn5Days : AttendanceReward
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardReturn5Days";

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.Prefab5DaysAuto;
	}
}
