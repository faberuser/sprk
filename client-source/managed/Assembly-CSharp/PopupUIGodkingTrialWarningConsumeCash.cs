using System;
using NGame2;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

public class PopupUIGodkingTrialWarningConsumeCash : BaseWindow
{
	public const string PATH_PREFAB = "Resources/Prefab2/Window/GodkingTrial/PopupUIGodkingTrialWarningConsumeCash";

	public UILabel LabelCash;

	public UILabel LabelRechargeCount;

	private int _maxCount = 1;

	private int _rechargeCount;

	private int _remainCount;

	private Action _onClickOk;

	private Action _onClose;

	public static void OpenGlobal(Action onClickOk, Action onClose = null)
	{
		WindowPrefab<PopupUIGodkingTrialWarningConsumeCash> windowPrefab = new WindowPrefab<PopupUIGodkingTrialWarningConsumeCash>("Resources/Prefab2/Window/GodkingTrial/PopupUIGodkingTrialWarningConsumeCash", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null)
		{
			windowPrefab.instance.Open(onClickOk, onClose);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public void OnClickOk()
	{
		Vespa_System_Action_Extension_Method.fire(_onClickOk);
		Close();
	}

	public void Open(Action onClickOk, Action onClose = null)
	{
		base.Open();
		_onClickOk = onClickOk;
		_onClose = onClose;
		SetData();
		UpdateUI();
	}

	public override void OnClosing()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.OnClosing();
	}

	private void SetData()
	{
		_maxCount = GodkingTrialHelper.GetTicketMaxResetCount();
		_rechargeCount = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.GodkingTrialKeyRechargeCount;
		_remainCount = Mathf.Max(0, _maxCount - _rechargeCount);
	}

	private void UpdateUI()
	{
		UpdateMessage();
		UpdateCashCount();
	}

	private void UpdateMessage()
	{
		string text = LocalizationManager.GetString(GodkingTrial.ASK_RECHARGE_TICKET_WITH_RUBY, GodkingTrialHelper.GetRechargeCount(), _remainCount, _maxCount);
		UIHelper.SetLabelText(LabelRechargeCount, text);
	}

	private void UpdateCashCount()
	{
		int result = 0;
		int num = Mathf.Min(_maxCount, _rechargeCount);
		int num2 = ((num >= 1) ? (num - 1) : 0);
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(StaminaType.GodkingTrialKey);
		if (data != null && Vespa_Generic_IList_Extension_Method.IsValidIndex(data.RechargeGemValue, num2))
		{
			int.TryParse(data.RechargeGemValue[num2], out result);
		}
		UIHelper.SetLabelText(LabelCash, result);
	}
}
