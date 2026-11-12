using System;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NGame2.NUI3.NMaxCap;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MaxCapWarningPopup : BaseWindow, IInspectorGuiDrawer
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MaxCapWarningPopup";

	public const string PrefabPath_AllMail = "Resources/Prefab2/Window/MessagePopup/MaxCapWarningPopup_AllMail";

	private const int MinWidth = 400;

	private const int MinHeight = 100;

	[Header("Max Cap Warning Popup")]
	public UIWidget RootWidget;

	public UITable MessageUiTable;

	protected Action _onYes;

	protected Action _onNo;

	public UILabel LabelYes;

	public UILabel LabelNo;

	private List<Component> _maxCapComponentList = new List<Component>();

	public static void Open(List<CurrencyType> currencyTypes, List<int> itemIndices, Action onYes, Action onNo)
	{
		WindowPrefab<MaxCapWarningPopup> windowPrefab = new WindowPrefab<MaxCapWarningPopup>("Resources/Prefab2/Window/MessagePopup/MaxCapWarningPopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(currencyTypes, itemIndices, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenForAllMail(List<CurrencyType> currencyTypes, List<int> itemIndices, Action onYes, Action onNo)
	{
		WindowPrefab<MaxCapWarningPopup> windowPrefab = new WindowPrefab<MaxCapWarningPopup>("Resources/Prefab2/Window/MessagePopup/MaxCapWarningPopup_AllMail", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(currencyTypes, itemIndices, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static GameUIRoot.ELayerType GetCurrentLayerType()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			MissionMilestonePopup.ReleaseWindow();
			return GameUIRoot.ELayerType.UI3DUI;
		}
		return GameUIRoot.ELayerType.UI;
	}

	protected void OpenInternal(List<CurrencyType> currencyTypes, List<int> itemIndices, Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(currencyTypes); i++)
		{
			CurrencyType currencyType = currencyTypes[i];
			CurrencyMaxCapComponent currencyMaxCapComponent = CurrencyMaxCapComponent.PrefabAuto.Factory.Create(MessageUiTable.transform);
			currencyMaxCapComponent.Open(currencyType);
			currencyMaxCapComponent.transform.SetSiblingIndex(__Extension_Method_ICollection__.COUNT(_maxCapComponentList) + 2);
			_maxCapComponentList.Add(currencyMaxCapComponent);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(itemIndices); j++)
		{
			int itemIndex = itemIndices[j];
			ItemMaxCapComponent itemMaxCapComponent = ItemMaxCapComponent.PrefabAuto.Factory.Create(MessageUiTable.transform);
			itemMaxCapComponent.Open(itemIndex);
			itemMaxCapComponent.transform.SetSiblingIndex(__Extension_Method_ICollection__.COUNT(_maxCapComponentList) + 2);
			_maxCapComponentList.Add(itemMaxCapComponent);
		}
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		UIHelper.SetLabelText(LabelYes, text);
		string text2 = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		UIHelper.SetLabelText(LabelNo, text2);
		SetupWindowSize();
	}

	protected void SetupWindowSize()
	{
		if (!(MessageUiTable == null))
		{
			MessageUiTable.Reposition();
			float num = CalculateHeight(MessageUiTable);
			RootWidget.height = (int)num;
			Vector3 localPosition = MessageUiTable.transform.localPosition;
			localPosition.y = (int)(num / 2f);
			MessageUiTable.transform.localPosition = localPosition;
			RootWidget.ForceUpdateAnchors(includeChildren: true);
		}
	}

	private static float CalculateHeight(UITable table)
	{
		Bounds bounds = default(Bounds);
		List<Transform> childList = table.GetChildList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform content = childList[i];
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(table.transform, content, considerInactive: true);
			bounds.Encapsulate(bounds2);
		}
		return bounds.size.y;
	}

	protected virtual void CalcBounds()
	{
		if (MessageUiTable == null)
		{
			return;
		}
		UIWidget component = MessageUiTable.GetComponent<UIWidget>();
		if (!(component == null))
		{
			Bounds bounds = default(Bounds);
			List<Transform> childList = MessageUiTable.GetChildList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
			{
				Transform content = childList[i];
				Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(component.transform, content, considerInactive: true);
				bounds.Encapsulate(bounds2);
			}
			component.width = (int)bounds.size.x;
			component.height = (int)bounds.size.y;
			component.ForceUpdateAnchors(includeChildren: true);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onYes = null;
		_onNo = null;
		FactoryBase.ReleaseAndClearList(_maxCapComponentList);
	}

	public override void Close()
	{
		Action onNo = _onNo;
		_onYes = null;
		_onNo = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
	}

	public void OnClickClose()
	{
		OnClickNo();
	}

	public void OnClickYes()
	{
		Action onYes = _onYes;
		_onYes = null;
		_onNo = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
	}

	public void OnClickNo()
	{
		Action onNo = _onNo;
		_onYes = null;
		_onNo = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
	}

	public override void OnSystemBackButton()
	{
		OnClickNo();
	}
}
