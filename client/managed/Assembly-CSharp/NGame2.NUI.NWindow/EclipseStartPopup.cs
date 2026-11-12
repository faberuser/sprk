using System;
using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EclipseStartPopup : BaseWindow, CountInputField.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/EclipseStartPopup";

	public UILabel LabelTitle;

	public UILabel LabelMessage;

	public UIButton ButtonOk;

	public UIButton ButtonClose;

	public CountInputField CountInputField;

	public UIGrid SortGird;

	public UIComboBox GameSpeedComboBox;

	public UISpriteSelector ComboBoxSelectItemIconSpriteSelector;

	private const int DefaultGameSpeed = 1;

	private Action<int, int> _onYes;

	private Action<int> _onYes2;

	private bool _isContinue;

	private int CurrentGameSpeed
	{
		get
		{
			if (GameSpeedComboBox == null)
			{
				return 1;
			}
			return GameSpeedComboBox.CurrentKey;
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onYes = null;
		_onYes2 = null;
	}

	public void Open(string title, bool isContinue, Action<int, int> onYes)
	{
		base.Open();
		_onYes = onYes;
		Init(title, isContinue);
	}

	public void Open(string title, bool isContinue, Action<int> onYes2)
	{
		base.Open();
		_onYes2 = onYes2;
		Init(title, isContinue);
	}

	private void Init(string title, bool isContinue)
	{
		_isContinue = isContinue;
		SetTitleText(title);
		SetMessageText();
		InitCountInputField(!isContinue);
		InitGameSpeed();
	}

	private void SetTitleText(string title)
	{
		UIHelper.SetLabelText(LabelTitle, title);
	}

	private void SetMessageText()
	{
		string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_INFO_POPUP_DESC");
		if (_isContinue)
		{
			stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_CONTINUE_INFO_POPUP_DESC");
		}
		UIHelper.SetLabelText(LabelMessage, stringDirect);
	}

	public void InitCountInputField(bool isOn)
	{
		UIHelper.EnableGameObject(CountInputField.transform, isOn);
		if (isOn)
		{
			SetCountInputField(GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey));
		}
		if (SortGird != null)
		{
			SortGird.Reposition();
		}
	}

	private void SetCountInputField(long maxCount)
	{
		if (CountInputField != null)
		{
			CountInputField countInputField = CountInputField;
			CountInputField.CountInputFieldInfo countInputFieldInfo = new CountInputField.CountInputFieldInfo();
			int num = 0;
			countInputFieldInfo.InitCount = ((maxCount > num) ? 1 : 0);
			countInputFieldInfo.MaxCount = maxCount;
			countInputFieldInfo.InitPrice = 0L;
			countInputField.Open(this, countInputFieldInfo);
		}
	}

	private void InitGameSpeed()
	{
		if (GameSpeedComboBox == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		int num = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo()?.OnlineGameSpeedRatio ?? 1;
		int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("EclipseMultiSpeed");
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(valueArray))
		{
			if (num <= 0)
			{
				num = 1;
			}
			GameSpeedComboBox.Clear();
			for (int i = 0; i < valueArray.Length; i++)
			{
				GameSpeedComboBox.AddItem(valueArray[i], LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_SPEED_OPTION_" + valueArray[i]));
			}
			GameSpeedComboBox.Reposition();
			GameSpeedComboBox.SelectItem(num);
			GameSpeedComboBox.OnChanged = ChangeGameSpeed;
			SetComboBoxGameSpeedIcon(num);
		}
	}

	private void ChangeGameSpeed(int value)
	{
		SetComboBoxGameSpeedIcon(value);
	}

	private void SetComboBoxGameSpeedIcon(int gameSpeed)
	{
		if (!(ComboBoxSelectItemIconSpriteSelector == null))
		{
			ComboBoxSelectItemIconSpriteSelector.Select(gameSpeed.ToString());
		}
	}

	private void SaveCurrentGameSpeed()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.SetGameSpeed(CurrentGameSpeed);
		}
	}

	public override void Close()
	{
		_onYes = null;
		_onYes2 = null;
		base.Close();
	}

	public void OnClickYes()
	{
		int currentGameSpeed = CurrentGameSpeed;
		if (_onYes != null && CountInputField != null)
		{
			Action<int, int> onYes = _onYes;
			long currentCount = CountInputField.CurrentCount;
			int i_argB = currentGameSpeed;
			long num = currentCount;
			Vespa_System_Action_Extension_Method.fire(onYes, checked((int)num), i_argB);
		}
		if (_onYes2 != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onYes2, currentGameSpeed);
		}
		SaveCurrentGameSpeed();
	}

	void CountInputField.IListener.ChangeCount(bool isMax)
	{
	}
}
