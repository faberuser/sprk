using NGame2.NUI.NComponent2;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardWelcome2020 : AttendanceReward
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardWelcome2020";

	public UILabel Label_PurchaseText;

	public UISimpleGrid ButtonGrid;

	public UIButton ButtonPurchase;

	protected bool IsButtonGridUpdate;

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabWelcome2020Auto;
	}

	protected override PrefabAuto<AttendanceRewardComponent> GetSpecialRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabWelcome2020SpecialAuto;
	}

	protected override void SetupCustom()
	{
		base.SetupCustom();
		SetupPurchaseUI();
	}

	protected virtual void SetupPurchaseUI()
	{
		bool enable = Helper.CanPurchaseAttendance(base.AttendanceInfo, base.AttendanceData);
		UIHelper.EnableGameObject(Label_PurchaseText, enable);
		UIHelper.EnableGameObject(ButtonPurchase, enable);
		IsButtonGridUpdate = true;
	}

	public override void ShowPurchaseButton(bool isShow)
	{
		UIHelper.EnableGameObject(ButtonPurchase, isShow);
		IsButtonGridUpdate = true;
	}

	private void LateUpdate()
	{
		if (IsButtonGridUpdate)
		{
			UIHelper.RepositionGrid(ButtonGrid);
			IsButtonGridUpdate = false;
		}
	}
}
