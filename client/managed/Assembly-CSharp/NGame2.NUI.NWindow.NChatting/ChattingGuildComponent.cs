using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingGuildComponent : ChattingListComponentBase
{
	public new interface Listener : ChattingListComponentBase.Listener
	{
		void OnSubmit(string chat);

		List<ChatInfo> GetGuildChat();
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Chatting/ChattingGuildPanelComponent";

	public const string BattlePrefabPath = "Resources/Prefab2/Component/BattleChatting/BattleChattingGuildPanelComponent";

	public UILabel NoGuildLabel;

	public UIButton SubmitButton;

	private Listener _listener;

	private ItemLinkButtonComponent ItemLinkButtonComponent;

	public GameObject ItemLinkButtonRoot;

	private bool NoGuild;

	public void Open(bool noGuild, Listener listener)
	{
		Open(listener);
		NoGuild = noGuild;
		if (!noGuild)
		{
			CreateDefaultChat();
		}
		_listener = listener;
		UIHelper.EnableGameObject(NoGuildLabel, noGuild);
		UIHelper.EnableButton(SubmitButton, !noGuild);
		UIHelper.EnableGameObject(Input, !noGuild);
		if (_listener != null && !noGuild)
		{
			_listener.GetGuildChat().ForEach(base.AddChat);
		}
		ItemLinkButtonComponent.AddComponent(ref ItemLinkButtonRoot, ref ItemLinkButtonComponent, ItemLinkButtonComponent.EItemLinkButtonType.Share, ChatType.Guild);
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
			if (NoGuild)
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
