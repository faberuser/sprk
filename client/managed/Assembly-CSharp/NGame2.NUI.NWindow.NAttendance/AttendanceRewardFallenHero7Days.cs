using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardFallenHero7Days : AttendanceReward
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardFallenHero7Days";

	public UIButton ButtonPurchase;

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabNew7DaysAuto;
	}

	protected override PrefabAuto<AttendanceRewardComponent> GetSpecialRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabNew7DaysSpecialAuto;
	}

	protected override void SetupCustom()
	{
		base.SetupCustom();
		bool enable = Helper.CanPurchaseAttendance(base.AttendanceInfo, base.AttendanceData);
		UIHelper.EnableGameObject(ButtonPurchase, enable);
	}

	public override void ShowPurchaseButton(bool isShow)
	{
		UIHelper.EnableGameObject(ButtonPurchase, isShow);
	}
}
