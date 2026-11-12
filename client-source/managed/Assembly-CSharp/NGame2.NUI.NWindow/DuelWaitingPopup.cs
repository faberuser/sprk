using NNguiExtension;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DuelWaitingPopup : BaseWindow
{
	public interface Listener
	{
		void OnCancel();
	}

	public enum EWaitingType
	{
		RequestDuel,
		WaitingPartySetting
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Match/DuelWaitingPopup";

	public int RequestWaitingTimeLen = 60;

	public int PartySettingWaitingTimeLen = 300;

	public UILabelSelector MessageSelector;

	public UILabel LabelMessage2;

	public UIButton CancelButton;

	public LocalizeLinkData Message2Localize = new LocalizeLinkData();

	private Listener _listener;

	private float _remainedTime;

	public void Open(EWaitingType waitingType, Listener listener, int waitingTime = 0)
	{
		base.Open();
		_listener = listener;
		_remainedTime = waitingTime;
		if (MessageSelector != null)
		{
			MessageSelector.SelectEnum(waitingType);
		}
		UIHelper.EnableGameObject(LabelMessage2, waitingTime > 0);
		UIHelper.SetWidgetGrayScale(CancelButton, waitingTime > 0, setColliders: true);
		UIHelper.SetLabelText(LabelMessage2, string.Empty);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_remainedTime = 0f;
		UIHelper.EnableGameObject(LabelMessage2, enable: false);
		UIHelper.SetWidgetGrayScale(CancelButton, grayScale: false, setColliders: true);
	}

	public override void Close()
	{
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!(_remainedTime <= 0f))
		{
			_remainedTime -= Time.deltaTime;
			if (_remainedTime <= 0f)
			{
				OnClickCancel();
				return;
			}
			string localize = Message2Localize.GetLocalize((int)_remainedTime);
			UIHelper.SetLabelText(LabelMessage2, localize);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickCancel();
	}

	public void OnClickCancel()
	{
		if (_listener != null)
		{
			_listener.OnCancel();
		}
		Close();
	}
}
