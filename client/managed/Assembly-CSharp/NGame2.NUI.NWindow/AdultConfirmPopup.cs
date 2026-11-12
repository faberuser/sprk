using System;
using System.Globalization;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class AdultConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/AdultConfirmPopup";

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIButton Button1;

	public UIButton Button2;

	public UILabel ButtonLabel1;

	public UILabel ButtonLabel2;

	public UIInput InputYear;

	public UIInput InputMonth;

	private Action _onButton1;

	private Action _onButton2;

	private const int ADULT_AGE = 20;

	public string Title { get; set; }

	public string Message { get; set; }

	public static void Open(string title, string message, Action onButton1, Action onButton2, Enum button1Label, Enum button2Label)
	{
		string button1Label2 = LocalizationManager.GetString(button1Label);
		string button2Label2 = LocalizationManager.GetString(button2Label);
		Open(title, message, onButton1, onButton2, button1Label2, button2Label2);
	}

	public static void Open(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label)
	{
		WindowPrefab<AdultConfirmPopup> windowPrefab = new WindowPrefab<AdultConfirmPopup>("Resources/Prefab2/Window/MessagePopup/AdultConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, button1Label, button2Label);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label)
	{
		base.Open();
		_onButton1 = onButton1;
		_onButton2 = onButton2;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.SetLabelText(ButtonLabel1, button1Label);
		UIHelper.SetLabelText(ButtonLabel2, button2Label);
		UIHelper.EnableButton(Button1, enable: false);
	}

	public override void Reset()
	{
		base.Reset();
		_onButton1 = null;
		_onButton2 = null;
		if (InputYear != null && InputMonth != null)
		{
			InputYear.value = "";
			InputMonth.value = "";
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (InputYear != null && InputMonth != null)
		{
			bool enable = !String_Extension_Method.IsNullOrEmpty(InputYear.text) && !String_Extension_Method.IsNullOrEmpty(InputMonth.text);
			UIHelper.EnableButton(Button1, enable);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	private void OnClickButton(Action onButton)
	{
		_onButton1 = null;
		_onButton2 = null;
		Close();
		Vespa_System_Action_Extension_Method.fire(onButton);
	}

	public void OnClickButton1()
	{
		int num = int.Parse(InputYear.value);
		int num2 = int.Parse(InputMonth.value);
		int day = 1;
		int InputAge = Age(num, num2, day);
		if (InputAge == -1)
		{
			MessagePopup.Open(LocalizationManager.GetStringDirect("COMMON_ERROR"), LocalizationManager.GetStringDirect("AGE_VERIFICATION_INVALID"));
			return;
		}
		MessagePopup3Button.Open(LocalizationManager.GetStringDirect("COMMON_CONFIRM"), LocalizationManager.GetStringDirect("AGE_VERIFICATION_CONFIRM", num, num2), null, delegate
		{
			if (IsAdult(InputAge))
			{
				SingletonUpdater<LoginManager>.instance.AdultAgreementAccepted.Value = true;
				OnClickButton(_onButton1);
			}
			else
			{
				AdultMessagePopup.Open(LocalizationManager.GetStringDirect("AGE_VERIFICATION_PARENTAL_CONSENT_TITLE"), LocalizationManager.GetStringDirect("AGE_VERIFICATION_PARENTAL_CONSENT"), delegate
				{
					SingletonUpdater<LoginManager>.instance.AdultAgreementAccepted.Value = true;
					OnClickButton(_onButton1);
				}, null, NShared.NLocalization.Common.COMMON_YES, NShared.NLocalization.Common.COMMON_NO);
			}
		}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
	}

	public void OnClickButton2()
	{
		OnClickButton(_onButton2);
	}

	private bool IsAdult(int myAge)
	{
		if (myAge < 20)
		{
			return false;
		}
		return true;
	}

	private int Age(int Year, int Month, int Day)
	{
		DateTime result = DateTime.Now;
		string[] formats = new string[2] { "yyyyMMdd", "yyyy-MM-dd" };
		if (!DateTime.TryParseExact($"{Year:0000}{Month:00}{Day:00}", formats, new CultureInfo("ja-JP"), DateTimeStyles.None, out result))
		{
			return -1;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (result > currentServerUTCTime || result.Year < 1900)
		{
			return -1;
		}
		int num = 0;
		int year = result.Year;
		int month = result.Month;
		int day = result.Day;
		int year2 = currentServerUTCTime.Year;
		int month2 = currentServerUTCTime.Month;
		int day2 = currentServerUTCTime.Day;
		if (month < month2)
		{
			return year2 - year;
		}
		if (month == month2)
		{
			if (day <= day2)
			{
				return year2 - year;
			}
			return year2 - year - 1;
		}
		return year2 - year - 1;
	}
}
