using System.Collections.Generic;
using NGame2.NChatting;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

[UIEvent(AllowDuringPatchDownloading = true)]
public class ChattingChannelComponent : ChattingListComponentBase
{
	public new interface Listener : ChattingListComponentBase.Listener
	{
		void OnChangeChannel();

		void OnSubmit(string chat);

		int GetRemainTime();

		bool IsCheckButtonState();

		List<ChatInfo> GetChannelChat();
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Chatting/ChattingChannelPanelComponent";

	public const string BattlePrefabPath = "Resources/Prefab2/Component/BattleChatting/BattleChattingChannelPanelComponent";

	public UIButton SubmitButton;

	public Transform DisabledRoot;

	public UILabel RemainTimeLabel;

	public UILabel ChannelNameLabel;

	private ItemLinkButtonComponent ItemLinkButtonComponent;

	public GameObject ItemLinkButtonRoot;

	private Listener _listener;

	public void Open(Listener listener)
	{
		Open((ChattingListComponentBase.Listener)listener);
		CreateDefaultChat();
		_listener = listener;
		if (_listener != null)
		{
			_listener.GetChannelChat()?.ForEach(base.AddChat);
		}
		GlobalAutoBehaviour<ChattingManager>.instance.AddCautionSystemChat();
		ItemLinkButtonComponent.AddComponent(ref ItemLinkButtonRoot, ref ItemLinkButtonComponent, ItemLinkButtonComponent.EItemLinkButtonType.Share, ChatType.Channel);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		Input.value = string.Empty;
	}

	public void OnClickChangeChannel()
	{
		if (_listener != null)
		{
			_listener.OnChangeChannel();
		}
	}

	public void OnClickSubmit()
	{
		if (_listener != null)
		{
			string value = Input.value;
			Input.value = string.Empty;
			value = value.Replace("\n", "").Replace("\r", "");
			_listener.OnSubmit(value);
		}
	}

	public void Update()
	{
		if (IsCheckUpdateSendButtonState())
		{
			UpdateSendButtonState();
		}
	}

	private bool IsCheckUpdateSendButtonState()
	{
		if (_listener == null)
		{
			return false;
		}
		return _listener.IsCheckButtonState();
	}

	private void UpdateSendButtonState()
	{
		int num = ((_listener != null) ? _listener.GetRemainTime() : 0);
		bool flag = num <= 0;
		UIHelper.EnableButton(SubmitButton, flag);
		UIHelper.EnableGameObject(Input, flag);
		UIHelper.EnableGameObject(DisabledRoot, !flag);
		string text = LocalizationManager.GetString(Chatting.CHAT_REMAIN_ENABLE_TIME, num);
		UIHelper.SetLabelText(RemainTimeLabel, text);
		if (ItemLinkButtonComponent != null)
		{
			ItemLinkButtonComponent.UpdateShareButtonState();
		}
	}

	public void SetChannelName(int channelNo)
	{
		UIHelper.SetLabelText(ChannelNameLabel, channelNo.ToString());
	}
}
