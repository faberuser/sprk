using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopAdultConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopAdultConfirmPopup";

	public UIButton ButtonYes;

	public UIInput InputYear;

	public UIInput InputMonth;

	private Action<bool> _onCallback;

	private const int ADULT_AGE = 20;

	public static void Open(Action<bool> callaback)
	{
		WindowPrefab<PayShopAdultConfirmPopup> windowPrefab = new WindowPrefab<PayShopAdultConfirmPopup>("Resources/Prefab2/Window/PayShop/PayShopAdultConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(callaback);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(Action<bool> callaback)
	{
		base.Open();
		_onCallback = callaback;
		UIHelper.EnableButton(ButtonYes, enable: false);
	}

	public override void Reset()
	{
		base.Reset();
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
			bool enable = !String_Extension_Method.IsNullOrEmpty(InputYear.value) && !String_Extension_Method.IsNullOrEmpty(InputMonth.value);
			UIHelper.EnableButton(ButtonYes, enable, grayscaleIfDisable: true);
		}
	}

	private void CallbackClose(bool isYes)
	{
		Action<bool> onCallback = _onCallback;
		_onCallback = null;
		Vespa_System_Action_Extension_Method.fire(onCallback, isYes);
		Close();
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void Close()
	{
		if (_onCallback != null)
		{
			Action<bool> onCallback = _onCallback;
			_onCallback = null;
			Vespa_System_Action_Extension_Method.fire(onCallback, i_argA: false);
		}
		base.Close();
	}

	public void OnClickButtonYes()
	{
		int year = int.Parse(InputYear.value);
		int month = int.Parse(InputMonth.value);
		int num = Age(year, month);
		if (num < 0)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_ERROR");
			string stringDirect2 = LocalizationManager.GetStringDirect("AGE_VERIFICATION_INVALID");
			PayShopMessagePopup.Open(stringDirect, stringDirect2);
		}
		else
		{
			ConfirmAge(year, month, num);
		}
	}

	private void ConfirmAge(int year, int month, int age)
	{
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_CONFIRM");
		string stringDirect2 = LocalizationManager.GetStringDirect("AGE_VERIFICATION_CONFIRM", year, month);
		PayShopMessage3ButtonPopup.Open(stringDirect, stringDirect2, delegate
		{
			if (IsAdult(age))
			{
				CallbackClose(isYes: true);
			}
			else
			{
				PayShopAdultMessagePopup.Open(delegate(bool isYes)
				{
					CallbackClose(isYes);
				});
			}
		});
	}

	private bool IsAdult(int myAge)
	{
		if (myAge < 20)
		{
			return false;
		}
		return true;
	}

	private int Age(int year, int month)
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (currentServerUTCTime.Year < year || (currentServerUTCTime.Year == year && currentServerUTCTime.Month < month))
		{
			return -1;
		}
		if (year < 1900)
		{
			return -1;
		}
		int num = currentServerUTCTime.Year - year;
		if (currentServerUTCTime.Month < month)
		{
			num--;
		}
		return num;
	}
}
