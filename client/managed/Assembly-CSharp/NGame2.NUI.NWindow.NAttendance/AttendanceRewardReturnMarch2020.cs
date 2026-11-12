using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardReturnMarch2020 : AttendanceRewardSupply14Days
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardReturnMarch2020";

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabMay14DaysAuto;
	}
}
