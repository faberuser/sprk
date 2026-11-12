using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NChatting;
using UnityEngine;

public static class SprkChatChannels
{
	public static int Normalize(int channel)
	{
		return Mathf.Clamp(channel, 1, 99);
	}

	public static void Configure(ChattingChannelSelect1 window)
	{
		window.minValue = 1;
		UIInput channelNoInput = window.ChannelNoInput;
		if (channelNoInput != null)
		{
			channelNoInput.value = Normalize(channelNoInput.value.TOINT(1)).ToString();
			channelNoInput.characterLimit = 2;
			channelNoInput.selectAllTextOnFocus = true;
			if (channelNoInput.GetComponent<SprkChatChannelInput>() == null)
			{
				channelNoInput.gameObject.AddComponent<SprkChatChannelInput>();
			}
		}
		Transform transform = window.transform.Find("Panel/Root/Toggle2/Label_ToggleLeft (1)");
		if (transform != null)
		{
			UILabel component = transform.GetComponent<UILabel>();
			if (component != null)
			{
				component.text = "(1-99)";
			}
		}
	}

	public static bool UsesKeyboardPolling(UIInput input)
	{
		if (input != null)
		{
			return input.GetComponent<SprkChatChannelInput>() != null;
		}
		return false;
	}

	public static void AddSendButton(ItemLinkButtonComponent component)
	{
		if (component == null || component.Button_Share == null || !component.Button_Share.gameObject.activeSelf)
		{
			return;
		}
		ChattingListComponentBase componentInParent = component.GetComponentInParent<ChattingListComponentBase>();
		if (!(componentInParent == null) && !(componentInParent.Input == null))
		{
			SprkChatSendButton sprkChatSendButton = component.GetComponent<SprkChatSendButton>();
			if (sprkChatSendButton == null)
			{
				sprkChatSendButton = component.gameObject.AddComponent<SprkChatSendButton>();
			}
			sprkChatSendButton.Initialize(component.Button_Share, componentInParent.Input);
		}
	}
}
