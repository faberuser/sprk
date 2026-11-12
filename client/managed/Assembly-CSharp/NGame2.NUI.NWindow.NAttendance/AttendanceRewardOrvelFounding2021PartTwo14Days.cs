using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardOrvelFounding2021PartTwo14Days : AttendanceRewardSupply14Days
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardOrvelFounding2021PartTwo14Days";

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabMay14DaysAuto;
	}
}
