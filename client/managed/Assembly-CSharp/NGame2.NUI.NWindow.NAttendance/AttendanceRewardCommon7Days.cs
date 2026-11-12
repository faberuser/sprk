using NGame2.NUI.NComponent2;
using NNguiExtension;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardCommon7Days : AttendanceReward
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardCommon7Days";

	public UISimpleGrid ButtonGrid;

	public UIButton ButtonPurchase;

	public UIBundleTexSelector HeroTexSelector;

	public UIBundleTexSelector TitleTexSelector;

	public UILabelSelector SubTitleLabelSelector;

	public UILabelSelector OfferLabelSelector;

	protected bool IsButtonGridUpdate;

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
		SetupPurchaseUI();
		SetSelector();
	}

	protected virtual void SetSelector()
	{
		string key = base.AttendanceData.Keyword.ToString();
		SetSelector(HeroTexSelector, key);
		SetSelector(TitleTexSelector, key);
		SetSelector(OfferLabelSelector, key);
		SetSelector(SubTitleLabelSelector, key);
	}

	public void SetSelector(UIBundleTexSelector selector, string key)
	{
		if (!(selector == null))
		{
			bool enable = selector.Select(key);
			UIHelper.EnableGameObject(selector, enable);
		}
	}

	public void SetSelector(UILabelSelector selector, string key)
	{
		if (!(selector == null))
		{
			bool enable = selector.Select(key);
			UIHelper.EnableGameObject(selector, enable);
		}
	}

	protected virtual void SetupPurchaseUI()
	{
		bool enable = Helper.CanPurchaseAttendance(base.AttendanceInfo, base.AttendanceData);
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
