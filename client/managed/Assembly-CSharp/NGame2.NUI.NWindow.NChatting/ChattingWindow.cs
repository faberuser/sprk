using System;
using System.Collections.Generic;
using NGame2.NParty;
using NGame2.NSocket;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingWindow : BaseWindow
{
	public interface Listener
	{
		void SaveLastChatInfo(ChatType chatType, long accountId);

		void ClearChatList(ChatType chatType);

		int GetAlertCount(ChatType chatType);

		void OnClose();

		void ReOpen();

		void OpenChattingWindow(ChattingWindow chattingWindow);
	}

	[Serializable]
	public class ToggleGroup
	{
		public UIToggle Toggle;

		public GameObject AlertObject;

		public UILabel AlertCountLabel;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingWindow";

	public const string BattlePrefabPath = "Resources/Prefab2/Window/Chatting/BattleChattingWindow";

	public ToggleGroup Channel;

	public ToggleGroup Guild;

	public ToggleGroup Party;

	public ToggleGroup Whisper;

	public UILabel ChannelNameLabel;

	public Transform ChatRoot;

	public Transform EmoticonSelectPopupRoot;

	private ChattingChannelComponent _channelComponent;

	private ChattingGuildComponent _guildComponent;

	private ChattingPartyComponent _partyComponent;

	private ChattingWhisperComponent _whisperComponent;

	private ChatType _toggle;

	private Listener _listener;

	private int _channelAlertCount;

	private int _guildAlertCount;

	private int _partyAlertCount;

	private int _whisperAlertCount;

	private LocalAccountEnum<ChatType> _storedToggle = new LocalAccountEnum<ChatType>(ChatType.Channel, typeof(ChattingWindow).FullName + "ChatType");

	public static PrefabAuto<ChattingChannelComponent> PrefabChannelAuto = new PrefabAuto<ChattingChannelComponent>("Resources/Prefab2/Component/Chatting/ChattingChannelPanelComponent");

	public static PrefabAuto<ChattingGuildComponent> PrefabGuildAuto = new PrefabAuto<ChattingGuildComponent>("Resources/Prefab2/Component/Chatting/ChattingGuildPanelComponent");

	public static PrefabAuto<ChattingPartyComponent> PrefabPartyAuto = new PrefabAuto<ChattingPartyComponent>("Resources/Prefab2/Component/Chatting/ChattingPartyPanelComponent");

	public static PrefabAuto<ChattingWhisperComponent> PrefabWhisperAuto = new PrefabAuto<ChattingWhisperComponent>("Resources/Prefab2/Component/Chatting/ChattingWhisperPanelComponent");

	public static PrefabAuto<ChattingChannelComponent> BattlePrefabChannelAuto = new PrefabAuto<ChattingChannelComponent>("Resources/Prefab2/Component/BattleChatting/BattleChattingChannelPanelComponent");

	public static PrefabAuto<ChattingGuildComponent> BattlePrefabGuildAuto = new PrefabAuto<ChattingGuildComponent>("Resources/Prefab2/Component/BattleChatting/BattleChattingGuildPanelComponent");

	public static PrefabAuto<ChattingPartyComponent> BattlePrefabPartyAuto = new PrefabAuto<ChattingPartyComponent>("Resources/Prefab2/Component/BattleChatting/BattleChattingPartyPanelComponent");

	public static PrefabAuto<ChattingWhisperComponent> BattlePrefabWhisperAuto = new PrefabAuto<ChattingWhisperComponent>("Resources/Prefab2/Component/BattleChatting/BattleChattingWhisperPanelComponent");

	public ChatType CurrentChatType => _toggle;

	public bool IsOpened(ChatType chatType)
	{
		if (!base.IsOpen())
		{
			return false;
		}
		return chatType == _toggle;
	}

	public void AddChat(ChatInfo chatInfo)
	{
		if (chatInfo.ChatType == ChatType.Channel && _channelComponent != null)
		{
			_channelComponent.AddChat(chatInfo);
			if (_channelComponent.IsOpen)
			{
				_channelComponent.Reposition();
			}
		}
		if (chatInfo.ChatType == ChatType.Guild && _guildComponent != null)
		{
			_guildComponent.AddChat(chatInfo);
			if (_guildComponent.IsOpen)
			{
				_guildComponent.Reposition();
			}
		}
		if (chatInfo.ChatType == ChatType.Party && _partyComponent != null)
		{
			_partyComponent.AddChat(chatInfo);
			if (_partyComponent.IsOpen)
			{
				_partyComponent.Reposition();
			}
		}
		if (chatInfo.ChatType == ChatType.Whisper && _whisperComponent != null)
		{
			_whisperComponent.AddChat(chatInfo);
			if (_whisperComponent.IsOpen)
			{
				_whisperComponent.Reposition();
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (SprkAccounts.UseFullChatLayout())
		{
			_channelComponent = PrefabChannelAuto.Factory.Create(ChatRoot, setActive: false);
			_guildComponent = PrefabGuildAuto.Factory.Create(ChatRoot, setActive: false);
			_partyComponent = PrefabPartyAuto.Factory.Create(ChatRoot, setActive: false);
			_whisperComponent = PrefabWhisperAuto.Factory.Create(ChatRoot, setActive: false);
		}
		else
		{
			_channelComponent = BattlePrefabChannelAuto.Factory.Create(ChatRoot, setActive: false);
			_guildComponent = BattlePrefabGuildAuto.Factory.Create(ChatRoot, setActive: false);
			_partyComponent = BattlePrefabPartyAuto.Factory.Create(ChatRoot, setActive: false);
			_whisperComponent = BattlePrefabWhisperAuto.Factory.Create(ChatRoot, setActive: false);
		}
	}

	public void Open(Listener listener, ChatType openChatType, long accountId)
	{
		Open(delegate
		{
			Init(listener, openChatType, accountId);
			if (listener != null)
			{
				listener.OpenChattingWindow(this);
			}
		}, null);
	}

	public void InitChannelName()
	{
		int channelNo = GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.ChannelNo;
		UIHelper.SetLabelText(ChannelNameLabel, channelNo.ToString());
		_channelComponent.SetChannelName(channelNo);
	}

	private void OpenComponent(ChatType toggle, long accountId)
	{
		switch (toggle)
		{
		case ChatType.Channel:
			_channelComponent.Open(SceneAutoBehaviour<ChattingManagement>.instance);
			_guildComponent.Close();
			_partyComponent.Close();
			_whisperComponent.Close();
			_channelComponent.Reposition(forceReset: true);
			break;
		case ChatType.Guild:
		{
			bool flag2 = GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.GuildId == 0;
			if (flag2)
			{
				_listener.ClearChatList(toggle);
			}
			_guildComponent.Open(flag2, SceneAutoBehaviour<ChattingManagement>.instance);
			_channelComponent.Close();
			_partyComponent.Close();
			_whisperComponent.Close();
			_guildComponent.Reposition(forceReset: true);
			break;
		}
		case ChatType.Party:
		{
			bool flag = !GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsValid();
			if (flag)
			{
				_listener.ClearChatList(toggle);
			}
			_partyComponent.Open(flag, SceneAutoBehaviour<ChattingManagement>.instance);
			_channelComponent.Close();
			_guildComponent.Close();
			_whisperComponent.Close();
			_partyComponent.Reposition(forceReset: true);
			break;
		}
		case ChatType.Whisper:
			_whisperComponent.Open(SceneAutoBehaviour<ChattingManagement>.instance);
			_whisperComponent.SelectUser(accountId);
			_channelComponent.Close();
			_guildComponent.Close();
			_partyComponent.Close();
			_whisperComponent.Reposition(forceReset: true);
			break;
		}
		InitChannelName();
		_listener.SaveLastChatInfo(toggle, 0L);
	}

	public void Init(Listener listener, ChatType openChatType, long accountId)
	{
		_listener = listener;
		if (openChatType == ChatType.None)
		{
			_toggle = _storedToggle.Value;
		}
		else
		{
			_toggle = openChatType;
		}
		MakeTopmostToggleGroup(Channel.Toggle, Guild.Toggle, Party.Toggle, Whisper.Toggle);
		Channel.Toggle.value = _toggle == ChatType.Channel;
		Guild.Toggle.value = _toggle == ChatType.Guild;
		Party.Toggle.value = _toggle == ChatType.Party;
		Whisper.Toggle.value = _toggle == ChatType.Whisper;
		OpenComponent(_toggle, accountId);
		SprkFixChatBackground();
		SprkAccounts.RepairChatTabs(this);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_channelAlertCount = -1;
		_guildAlertCount = -1;
		_partyAlertCount = -1;
		_whisperAlertCount = -1;
		_channelComponent.Reset();
		_guildComponent.Reset();
		_partyComponent.Reset();
		_whisperComponent.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateAlertCount(ChatType.Channel, Channel, ref _channelAlertCount);
		UpdateAlertCount(ChatType.Guild, Guild, ref _guildAlertCount);
		UpdateAlertCount(ChatType.Party, Party, ref _partyAlertCount);
		UpdateAlertCount(ChatType.Whisper, Whisper, ref _whisperAlertCount);
	}

	private void UpdateAlertCount(ChatType chatType, ToggleGroup toggleGroup, ref int alertCount)
	{
		if (_listener != null)
		{
			int alertCount2 = _listener.GetAlertCount(chatType);
			if (alertCount2 != alertCount)
			{
				alertCount = alertCount2;
				UIHelper.EnableGameObject(toggleGroup.AlertObject, alertCount > 0);
				UIHelper.SetLabelText(toggleGroup.AlertCountLabel, alertCount);
			}
		}
	}

	public long GetCurrentAccountId()
	{
		return _whisperComponent.GetCurrentAccountId();
	}

	public void AddUser(long accountId, string userName)
	{
		_whisperComponent.AddUser(accountId, userName);
	}

	public void SelectUser(long senderId)
	{
		_whisperComponent.SelectUser(senderId);
	}

	public void SelectUser(string userName)
	{
		_whisperComponent.SelectUser(userName);
	}

	public bool HasReceiver(string userName)
	{
		return _whisperComponent.HasReceiver(userName);
	}

	public void RemoveUser(long accountId)
	{
		_whisperComponent.RemoveUser(accountId);
	}

	public long GetFirstAccountId()
	{
		return _whisperComponent.GetFirstAccountId();
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.OnClose();
		}
	}

	public void OnClickBackground()
	{
		OnClickClose();
	}

	public void OnToggleChanged()
	{
		ChatType chatType = ChatType.None;
		if (Channel.Toggle.value)
		{
			chatType = ChatType.Channel;
		}
		if (Guild.Toggle.value)
		{
			chatType = ChatType.Guild;
		}
		if (Party.Toggle.value)
		{
			chatType = ChatType.Party;
		}
		if (Whisper.Toggle.value)
		{
			chatType = ChatType.Whisper;
		}
		if (chatType != ChatType.None && _toggle != chatType)
		{
			_toggle = chatType;
			_storedToggle.Value = _toggle;
			OpenComponent(chatType, 0L);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if (_listener != null)
		{
			_listener.ReOpen();
		}
	}

	private IEnumerable<ChattingListComponentBase> GetChattingListComponents()
	{
		yield return _channelComponent;
		yield return _guildComponent;
		yield return _partyComponent;
		yield return _whisperComponent;
	}

	public ChattingListComponentBase GetCurrentChattingComponent()
	{
		if (_toggle == ChatType.Channel)
		{
			return _channelComponent;
		}
		if (_toggle == ChatType.Guild)
		{
			return _guildComponent;
		}
		if (_toggle == ChatType.Party)
		{
			return _partyComponent;
		}
		if (_toggle == ChatType.Whisper)
		{
			return _whisperComponent;
		}
		return null;
	}

	private void SprkFixChatBackground()
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return;
		}
		Transform obj = base.transform.Find("Panel/Root/Sprite_Frame_a");
		if ((object)obj != null)
		{
			UISprite component = obj.GetComponent<UISprite>();
			if ((object)component != null)
			{
				component.spriteName = "blank_white";
				component.color = new Color(0.965f, 0.937f, 0.875f, 1f);
			}
		}
	}
}
