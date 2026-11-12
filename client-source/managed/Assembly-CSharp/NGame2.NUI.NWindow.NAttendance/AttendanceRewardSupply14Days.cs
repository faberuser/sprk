using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceRewardSupply14Days : AttendanceReward
{
	[Serializable]
	public class PurchaseDescInfo
	{
		public EAttendanceKeyword Keyword;

		public string PurchaseDesc;
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceRewardSupply14Days";

	public UILabel Label_PurchaseText;

	public UISimpleGrid ButtonGrid;

	public UIButton ButtonPurchase;

	public UILabel PurchaseDescLabel;

	public List<PurchaseDescInfo> PurchaseDescInfoList = new List<PurchaseDescInfo>();

	public UIBundleTexSelector HeroTexSelector;

	public UIBundleTexSelector TitleTexSelector;

	public UILabelSelector SubTitleLabelSelector;

	public UILabelSelector OfferLabelSelector;

	protected bool IsButtonGridUpdate;

	protected override PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.Prefab14DaysAuto;
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
		UIHelper.EnableGameObject(Label_PurchaseText, enable);
		UIHelper.EnableGameObject(ButtonPurchase, enable);
		SetPurchaseDescLabel();
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

	private void SetPurchaseDescLabel()
	{
		string text = "";
		UIHelper.SetLabelText(PurchaseDescLabel, text);
		if (base.AttendanceData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(PurchaseDescInfoList))
		{
			PurchaseDescInfo purchaseDescInfo = PurchaseDescInfoList.Find((PurchaseDescInfo e) => e.Keyword == base.AttendanceData.Keyword);
			if (purchaseDescInfo != null)
			{
				text = LocalizationManager.GetStringDirect(purchaseDescInfo.PurchaseDesc);
				UIHelper.SetLabelText(PurchaseDescLabel, text);
			}
		}
	}
}
