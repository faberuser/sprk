using NGame2.NUI.NComponent2;
using NGame2.NUI3.NAttendance;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceReward9Chapter14Days : AttendanceRewardSupply14Days
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceReward9Chapter14Days";

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.Prefab9Chatper14DaysAuto;
	}

	protected override void SetupPurchaseUI()
	{
		bool enable = Helper.CanPurchaseAttendance(base.AttendanceInfo, base.AttendanceData);
		if (AttendanceHelper.IsPayShopProductSelling(base.AttendanceData.Index))
		{
			UIHelper.EnableGameObject(Label_PurchaseText, enable);
		}
		else
		{
			UIHelper.EnableGameObject(Label_PurchaseText, enable: true);
		}
		UIHelper.EnableGameObject(ButtonPurchase, enable);
		IsButtonGridUpdate = true;
	}
}
