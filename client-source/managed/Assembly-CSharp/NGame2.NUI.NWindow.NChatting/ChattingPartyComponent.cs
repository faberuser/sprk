using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingPartyComponent : ChattingListComponentBase
{
	public new interface Listener : ChattingListComponentBase.Listener
	{
		void OnSubmit(string chat);

		List<ChatInfo> GetPartyChat();
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Chatting/ChattingPartyPanelComponent";

	public const string BattlePrefabPath = "Resources/Prefab2/Component/BattleChatting/BattleChattingPartyPanelComponent";

	public UIButton SubmitButton;

	public UILabel NoPartyLabel;

	private Listener _listener;

	private ItemLinkButtonComponent ItemLinkButtonComponent;

	public GameObject ItemLinkButtonRoot;

	private bool NoParty;

	public void Open(bool noParty, Listener listener)
	{
		Open(listener);
		NoParty = noParty;
		if (!noParty)
		{
			CreateDefaultChat();
		}
		_listener = listener;
		UIHelper.EnableGameObject(NoPartyLabel, noParty);
		UIHelper.EnableButton(SubmitButton, !noParty);
		UIHelper.EnableGameObject(Input, !noParty);
		if (_listener != null && !noParty)
		{
			_listener.GetPartyChat()?.ForEach(base.AddChat);
		}
		ItemLinkButtonComponent.AddComponent(ref ItemLinkButtonRoot, ref ItemLinkButtonComponent, ItemLinkButtonComponent.EItemLinkButtonType.Share, ChatType.Party);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		Input.value = string.Empty;
	}

	public void OnClickSubmit()
	{
		if (_listener != null)
		{
			string value = Input.value;
			Input.value = string.Empty;
			_listener.OnSubmit(value);
		}
	}

	public void Update()
	{
		UpdateEmoticonSelectButtonState();
		if (ItemLinkButtonComponent != null)
		{
			if (NoParty)
			{
				ItemLinkButtonComponent.UpdateShareButtonState(enable: false);
			}
			else
			{
				ItemLinkButtonComponent.UpdateShareButtonState(enable: true);
			}
		}
	}
}
