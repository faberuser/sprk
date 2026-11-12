using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EventGuidePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Help/EventGuidePopup";

	public int WidgetCount = 3;

	public UILabel Lbl_Count;

	public UIButton Btn_EventShortCut;

	public UILabel Label_EventShortCutBtn;

	public UILabel Label_WindowTitle;

	public UILabel[] Lbl_Title;

	public UILabel[] Lbl_Desc;

	public UIWidget[] WidgetUi;

	public UITable Table_Root;

	public UIScrollView GuideScrollView;

	public static void Open(string[] openHelpString)
	{
		WindowPrefab<EventGuidePopup> windowPrefab = new WindowPrefab<EventGuidePopup>("Resources/Prefab2/Window/Help/EventGuidePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(openHelpString);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(Enum[] enums)
	{
		WindowPrefab<EventGuidePopup> windowPrefab = new WindowPrefab<EventGuidePopup>("Resources/Prefab2/Window/Help/EventGuidePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(enums);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string[] openHelpString)
	{
		base.Open();
		Init(openHelpString);
	}

	protected void OpenInternal(Enum[] enums)
	{
		Open(delegate
		{
			Init(enums);
		}, null);
	}

	private string GetEnumConverLocalizeString(Enum[] enums, int index)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(enums))
		{
			return null;
		}
		if (index < 0 || index >= __Extension_Method_ICollection__.COUNT(enums))
		{
			return null;
		}
		return LocalizationManager.GetString(enums[index]);
	}

	private void Init(Enum[] enums)
	{
		UIHelper.SetLabelText(Label_WindowTitle, GetEnumConverLocalizeString(enums, 0));
		for (int i = 0; i < WidgetCount; i++)
		{
			int num = 2 * i - 1;
			string enumConverLocalizeString = GetEnumConverLocalizeString(enums, num);
			string enumConverLocalizeString2 = GetEnumConverLocalizeString(enums, num + 1);
			if (String_Extension_Method.IsNullOrEmpty(enumConverLocalizeString) || String_Extension_Method.IsNullOrEmpty(enumConverLocalizeString2))
			{
				UIHelper.EnableGameObject(WidgetUi[i], enable: false);
				continue;
			}
			UIHelper.EnableGameObject(WidgetUi[i], enable: true);
			UIHelper.SetLabelText(Lbl_Title[i], LocalizationManager.GetStringDirect(enumConverLocalizeString));
			UIHelper.SetLabelText(Lbl_Desc[i], LocalizationManager.GetStringDirect(enumConverLocalizeString2));
		}
		UIHelper.EnableGameObject(Btn_EventShortCut, enable: false);
		SetRestrictEventPoint();
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, RepostionNextUpdate());
	}

	private void Init(string[] openHelpString)
	{
		UIHelper.SetLabelText(Label_WindowTitle, LocalizationManager.GetStringDirect(openHelpString[0]));
		for (int i = 0; i < WidgetCount; i++)
		{
			int num = 2 * (i + 1);
			string valueString = ArgParser.GetValueString(openHelpString, num);
			string valueString2 = ArgParser.GetValueString(openHelpString, num + 1);
			if (String_Extension_Method.IsNullOrEmpty(valueString) || String_Extension_Method.IsNullOrEmpty(valueString2))
			{
				UIHelper.EnableGameObject(WidgetUi[i], enable: false);
				continue;
			}
			UIHelper.EnableGameObject(WidgetUi[i], enable: true);
			UIHelper.SetLabelText(Lbl_Title[i], LocalizationManager.GetStringDirect(valueString));
			UIHelper.SetLabelText(Lbl_Desc[i], LocalizationManager.GetStringDirect(valueString2));
		}
		SetRestrictEventPoint();
		SetEventShortCutButton();
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, RepostionNextUpdate());
	}

	private IEnumerator RepostionNextUpdate()
	{
		yield return null;
		if ((bool)Table_Root)
		{
			Table_Root.repositionNow = true;
		}
		if ((bool)GuideScrollView)
		{
			GuideScrollView.ResetPosition();
		}
	}

	public void SetRestrictEventPoint()
	{
		CurrencyTypeData currencyData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.EventDungeonPoint);
		if (currencyData.DailyLimitValue <= 0)
		{
			UIHelper.EnableGameObject(Lbl_Count, enable: false);
			return;
		}
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.EventDungeonDailyPoint);
		UIHelper.EnableGameObject(Lbl_Count, enable: true);
		UIHelper.SetLabelText(Lbl_Count, LocalizationManager.GetStringDirect("HELP_NEWYEAR_EVENT_CONTENT_COUNT", userStat, currencyData.DailyLimitValue));
	}

	public void SetEventShortCutButton()
	{
		if (1 == 0)
		{
			UIHelper.EnableGameObject(Btn_EventShortCut, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Btn_EventShortCut, enable: false);
		UIHelper.SetLabelText(Label_EventShortCutBtn, LocalizationManager.GetStringDirect("HELP_NEWYEAR_EVENT_CONTENT_COUNT2"));
	}

	public void OnClickEventShortCutButton()
	{
		if (true)
		{
			base.Close();
			EventWebViewPopup.OpenGlobal();
		}
	}
}
