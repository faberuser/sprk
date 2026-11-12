using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardMay14Days : AttendanceRewardSupply14Days
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardMay14Days";

	public const string PrefabPathMagicKingdomPart1 = "Resources/Prefab2/Window/Attendance/AttendanceReward14DaysMagicKingdomPart1";

	public const string PrefabPathMagicKingdomPart2 = "Resources/Prefab2/Window/Attendance/AttendanceReward14DaysMagicKingdomPart2";

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabMay14DaysAuto;
	}
}
