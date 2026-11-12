using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NMessageLog;
using NGame2.NSocket;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NShared.NMessage;
using NShared.NMessage.NProtocol;
using NShared.SearchFriend;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class ChattingManagement : BaseManagement<ChattingManagement>, ChattingWindow.Listener, ChattingChannelComponent.Listener, ChattingListComponentBase.Listener, ChattingChannelSelect1.Listener, ChattingGuildComponent.Listener, ChattingPartyComponent.Listener, ChattingWhisperComponent.Listener, ChattingWhisperTarget.Listener, ChattingEmoticonSelectPopup.Listener, IChatListener, IChannelListener, ItemLinkButtonComponent.Listener
{
	private long _lastWhisperAccountId;

	private Dictionary<ChatType, DateTime> _lastSendTimeDic = new Dictionary<ChatType, DateTime>();

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		if (SprkAccounts.UseFullChatLayout())
		{
			AddInstance<ChattingWindow>("Resources/Prefab2/Window/Chatting/ChattingWindow");
		}
		else
		{
			AddInstance<BattleChat>("Resources/Prefab2/Window/Battle/BattleChat");
			AddInstance<ChattingWindow>("Resources/Prefab2/Window/Chatting/BattleChattingWindow");
		}
		AddInstance<ChattingChannelSelect1>("Resources/Prefab2/Window/Chatting/ChattingChannelSelect_1");
		AddInstance<ChattingWhisperTarget>("Resources/Prefab2/Window/Chatting/ChattingWhisperTarget");
		AddInstance<InspectEquipInfo>("Resources/Prefab2/Window/Hero/InspectEquipInfo");
		GlobalAutoBehaviour<ChattingManager>.instance.AddChannelListener(this);
		GlobalAutoBehaviour<ChattingManager>.instance.AddChatListener(this);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			GlobalAutoBehaviour<ChattingManager>.instance.RemoveChannelListener(this);
			GlobalAutoBehaviour<ChattingManager>.instance.RemoveChatListener(this);
			ChattingEmoticonSelectPopup.CloseAndResetComponent();
		}
	}

	public void OpenChatting(ChatType openChatType = ChatType.None, long accountId = 0L)
	{
		CloseBut<ChattingWindow>();
		SwitchToggleButton(isOpenChatWindow: true);
		ChattingPreview.AbledChating();
		ChattingWindow chattingWindow = GetInstance<ChattingWindow>();
		if (chattingWindow.IsOpen())
		{
			chattingWindow.ShowRoot(show: true);
			chattingWindow.Init(this, openChatType, accountId);
		}
		else
		{
			chattingWindow.Open(this, openChatType, accountId);
		}
	}

	public void ToggleChatting(ChatType openChatType = ChatType.None, long accountId = 0L)
	{
		if (IsOpened<ChattingWindow>())
		{
			GetInstance<ChattingWindow>().OnClickClose();
		}
		else
		{
			OpenChatting(openChatType, accountId);
		}
	}

	public void SwitchToggleButton(bool isOpenChatWindow)
	{
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			if (isOpenChatWindow)
			{
				SceneAutoBehaviour<BattleManagement>.instance.CloseInstance<BattleChat>();
				GetInstance<BattleChat>().Open();
			}
			else
			{
				SceneAutoBehaviour<BattleManagement>.instance.GetInstance<BattleChat>().Open();
				CloseInstance<BattleChat>();
			}
		}
	}

	public int GetChatRemainTime(ChatType chatType)
	{
		if (GameRoot.Instance == null || GameRoot.Instance.ConstantDataContainer == null)
		{
			return 0;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.AdminLevel >= GetAdminRequireLevel())
		{
			return 0;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime lastSendTime = GetLastSendTime(chatType);
		int num = (int)(utcNow - lastSendTime).TotalMilliseconds;
		return Mathf.Max((GameRoot.Instance.ConstantDataContainer.GetValueInt("ChannelChattingCooltime") * 1000 - num + 999) / 1000, 0);
	}

	public Transform GetEmotionSelectPopupRoot()
	{
		ChattingWindow chattingWindow = GetInstance<ChattingWindow>();
		if (chattingWindow == null)
		{
			return null;
		}
		return chattingWindow.EmoticonSelectPopupRoot;
	}

	public void OpenBattleEmoticonSelectPopup(ChattingEmoticonSelectPopup.Listener listener, BattleType battleType)
	{
		ChattingEmoticonSelectPopup.Open(listener, delegate(EmoticonData emoticonData)
		{
			string chat = "(" + emoticonData.Keyword + ")";
			if (battleType == BattleType.Arena || battleType == BattleType.ArenaBanPick)
			{
				if (emoticonData != null)
				{
					if (GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.State != ConnectState.Connected)
					{
						if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
						{
							long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
							SceneAutoBehaviour<BattleManagement>.instance.ShowBattleEmoticon(accountId, emoticonData.Index);
						}
					}
					else
					{
						BattleMessageReq data = new BattleMessageReq
						{
							Message = emoticonData.Index.ToString()
						};
						GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(data, delegate(BattleMessageRes res)
						{
							if (res.Result != BattleMessageRes.ResultType.Success && SceneAutoBehaviour<BattleManagement>.isValidInstance)
							{
								long accountId2 = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
								SceneAutoBehaviour<BattleManagement>.instance.ShowBattleEmoticon(accountId2, emoticonData.Index);
							}
						});
					}
				}
			}
			else
			{
				SendPartyChatShowBattleEmoticon(chat);
			}
		});
	}

	public void OpenEmoticonSelectPopup()
	{
		ChattingWindow chattingWindow = GetInstance<ChattingWindow>();
		if (chattingWindow == null || !chattingWindow.IsOpen())
		{
			return;
		}
		ChattingListComponentBase chattingComponent = chattingWindow.GetCurrentChattingComponent();
		if (!(chattingComponent == null) && UIHelper.IsEnabledGameObjet(chattingComponent.Input))
		{
			ChattingEmoticonSelectPopup.Open(this, delegate(EmoticonData emoticonData)
			{
				string value = "(" + emoticonData.Keyword + ")";
				chattingComponent.Input.value = value;
				chattingComponent.Input.selectAllTextOnFocus = false;
				chattingComponent.Input.isSelected = true;
			});
		}
	}

	private void RecordLastSendTime(ChatType chatType)
	{
		_lastSendTimeDic[chatType] = DateTime.UtcNow;
	}

	private DateTime GetLastSendTime(ChatType chatType)
	{
		_lastSendTimeDic.TryGetValue(chatType, out var value);
		return value;
	}

	public void OnEventCamera(EventCamera.EventValue current)
	{
		float uiAlpha = current.UiAlpha;
		if (IsOpened<BattleChat>())
		{
			GetInstance<BattleChat>().SetEventHide(uiAlpha);
		}
	}

	void IChatListener.OnChatAdded(ChatInfo chatInfo, bool showPreview, EmotiType emoticon)
	{
		ChatType chatType = chatInfo.ChatType;
		if (!IsChatWindowOpened(chatType))
		{
			if (showPreview)
			{
				if (emoticon == EmotiType.None)
				{
					ChattingPreview.Open(chatInfo);
				}
				else
				{
					EmoticonChattingPreview.Open(chatInfo, emoticon);
				}
			}
			return;
		}
		switch (chatType)
		{
		case ChatType.Channel:
		case ChatType.Guild:
		case ChatType.Party:
			SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>().AddChat(chatInfo);
			GlobalAutoBehaviour<ChattingManager>.instance.MakeChatChecked(chatType, 0L);
			break;
		case ChatType.Whisper:
		{
			long num = chatInfo.SenderId;
			string userName = chatInfo.SenderName;
			if (num == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				num = chatInfo.ReceiverId;
				userName = chatInfo.ReceiverName;
			}
			ChattingWindow chattingWindow = SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>();
			if (chattingWindow.GetCurrentAccountId() == num)
			{
				chattingWindow.AddChat(chatInfo);
				GlobalAutoBehaviour<ChattingManager>.instance.MakeChatChecked(chatType, num);
				break;
			}
			chattingWindow.AddUser(num, userName);
			if (chattingWindow.GetCurrentAccountId() == 0L)
			{
				chattingWindow.SelectUser(num);
				GlobalAutoBehaviour<ChattingManager>.instance.MakeChatChecked(chatType, num);
			}
			break;
		}
		}
	}

	void IChatListener.OnWhisperUserAdded(long accountId, string userName)
	{
		SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>().AddUser(accountId, userName);
	}

	void IChatListener.OnWhisperUserRemoved(long accountId)
	{
		SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>().RemoveUser(accountId);
	}

	void IChannelListener.OnChannelEnter(int channelNo)
	{
		if (IsChatWindowOpened(ChatType.Channel))
		{
			GetInstance<ChattingWindow>().InitChannelName();
		}
	}

	void IChannelListener.OnChannelLeave()
	{
	}

	public bool IsChatWindowOpened(ChatType chatType)
	{
		ChattingWindow chattingWindow = GetInstance<ChattingWindow>();
		if (chattingWindow != null)
		{
			return chattingWindow.IsOpened(chatType);
		}
		return false;
	}

	private static T CreateSendingChat<T>(long receiverId, string receiverName, string chat, LinkedItemInfo[] lastLinkedItem = null, int emoticonIndex = 0, bool isShowBattleEmoti = false) where T : BaseChat, new()
	{
		return new T
		{
			SenderName = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick,
			SenderAvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex,
			SenderTeamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel,
			ReceiverId = receiverId,
			ReceiverName = receiverName,
			SendTime = Vespa_DateTime_Extension_Method.ToNetworkString2(DateTime.UtcNow),
			Chat = chat,
			AdminLevel = GlobalAutoBehaviour<AccountManager>.instance.AdminLevel,
			LinkedItem = lastLinkedItem,
			EmoticonIndex = emoticonIndex,
			IsShowBattleEmoti = isShowBattleEmoti
		};
	}

	private static string ReplaceCensoredWord(string text)
	{
		return GameRoot.Instance.CensoredWordDataContainer.ReplaceCensoredWord(text, isChat: true);
	}

	private void ChatProcess(ChatType chatType, long toAccountId, string receiverName, string chat, LinkedItemInfo[] lastLinkedItem, bool isShowBattleEmoti)
	{
		if (!string.IsNullOrEmpty(chat) && !GlobalAutoBehaviour<ChattingManager>.instance.ProcessCheatCommand(chatType, chat))
		{
			if (GlobalAutoBehaviour<ChattingManager>.instance.IsChatBan(chatType))
			{
				ChatBanType chatBanType = ChattingManager.GetChatBanType(chatType);
				chat = GlobalAutoBehaviour<ChattingManager>.instance.GetChatBanErrorString(chatBanType);
				GlobalAutoBehaviour<ChattingManager>.instance.AddSystemChat(chat, chatType, showPreview: false, 0L);
			}
			else if (!ServerLanguageChecker.CheckValidCharacter(chat))
			{
				CenterMessage.OpenError(Chatting.CHAT_UNDEFINED_LANGUAGE);
			}
			else
			{
				chat = ReplaceCensoredWord(chat);
				SendChat(chatType, toAccountId, receiverName, chat, lastLinkedItem, isShowBattleEmoti);
			}
		}
	}

	private void SendChat(ChatType chatType, long toAccountId, string receiverName, string chat, LinkedItemInfo[] lastLinkedItem, bool isShowBattleEmoti)
	{
		int emoticonIndex = 0;
		if (chat.StartsWith("("))
		{
			int num = chat.IndexOf(")");
			if (num >= 0)
			{
				string keyword = chat.Substring(1, num - 1);
				emoticonIndex = GameRoot.Instance.EmoticonDataContainer.GetDataByKeyword(keyword)?.Index ?? 0;
				chat = chat.Substring(num + 1);
			}
		}
		switch (chatType)
		{
		case ChatType.Channel:
			SingletonAuto<MessageLogHandler>.instance.SendToChannel(CreateSendingChat<ChannelChat>(0L, null, chat, lastLinkedItem, emoticonIndex, isShowBattleEmoti));
			break;
		case ChatType.Party:
			SingletonAuto<MessageLogHandler>.instance.SendToParty(CreateSendingChat<PartyChat>(0L, null, chat, lastLinkedItem, emoticonIndex, isShowBattleEmoti));
			break;
		case ChatType.Guild:
			SingletonAuto<MessageLogHandler>.instance.SendToGuild(CreateSendingChat<GuildChat>(0L, null, chat, lastLinkedItem, emoticonIndex, isShowBattleEmoti));
			break;
		case ChatType.Whisper:
		{
			long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			if (id == toAccountId)
			{
				VespaDebug.LogError("Cannot whisper to me!");
				break;
			}
			SingletonAuto<MessageLogHandler>.instance.SendToClient(new long[2] { toAccountId, id }, CreateSendingChat<WhisperChat>(toAccountId, receiverName, chat, lastLinkedItem, emoticonIndex, isShowBattleEmoti));
			break;
		}
		}
	}

	void ChattingWindow.Listener.OnClose()
	{
		AddTask(base.TaskFactory.Run(delegate
		{
			CloseBut();
		}));
		SwitchToggleButton(isOpenChatWindow: false);
	}

	int ChattingWindow.Listener.GetAlertCount(ChatType chatType)
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetAlertCount(chatType, 0L);
	}

	void ChattingWindow.Listener.ReOpen()
	{
	}

	void ChattingWindow.Listener.OpenChattingWindow(ChattingWindow chattingWindow)
	{
		if (chattingWindow != null)
		{
			if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
			{
				LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
				chattingWindow.ChangeLayerRecursive(layerInfo);
			}
			else
			{
				LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
				chattingWindow.ChangeLayerRecursive(layerInfo2);
			}
		}
	}

	void ChattingWindow.Listener.SaveLastChatInfo(ChatType chatType, long accountId)
	{
		GlobalAutoBehaviour<ChattingManager>.instance.MakeChatChecked(chatType, accountId);
	}

	void ChattingWindow.Listener.ClearChatList(ChatType chatType)
	{
		GlobalAutoBehaviour<ChattingManager>.instance.ClearChatList(chatType, 0L);
	}

	void ChattingListComponentBase.Listener.OnClickChatUser(ChatInfo chatInfo)
	{
		if (chatInfo.SenderId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(chatInfo.SenderId, chatInfo.SenderAvaterIndex, chatInfo.SenderTeamLevel, chatInfo.SenderName, InspectUserCategory.General);
			SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
		}
	}

	void ChattingChannelComponent.Listener.OnChangeChannel()
	{
		int channelNo = GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.ChannelNo;
		base.TaskFactory.Run(delegate(ChattingChannelSelect1 window)
		{
			window.Open(this, channelNo);
		}).Commit();
	}

	int ChattingChannelComponent.Listener.GetRemainTime()
	{
		return GetChatRemainTime(ChatType.Channel);
	}

	public int GetAdminRequireLevel()
	{
		int defaultValue = 2;
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("AdminRequireLevel", defaultValue);
	}

	void ChattingChannelComponent.Listener.OnSubmit(string chat)
	{
		ChatProcess(ChatType.Channel, 0L, null, chat, null, isShowBattleEmoti: false);
		RecordLastSendTime(ChatType.Channel);
	}

	List<ChatInfo> ChattingChannelComponent.Listener.GetChannelChat()
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetChatList(ChatType.Channel, 0L);
	}

	bool ChattingChannelComponent.Listener.IsCheckButtonState()
	{
		return true;
	}

	void ChattingListComponentBase.Listener.OnClickEmoticonSelect()
	{
		OpenEmoticonSelectPopup();
	}

	void ChattingChannelSelect1.Listener.OnSelectChannel(int channelNo)
	{
		GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.ChangeChannel(channelNo);
	}

	void ChattingGuildComponent.Listener.OnSubmit(string chat)
	{
		ChatProcess(ChatType.Guild, 0L, null, chat, null, isShowBattleEmoti: false);
	}

	List<ChatInfo> ChattingGuildComponent.Listener.GetGuildChat()
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetChatList(ChatType.Guild, 0L);
	}

	public void SendPartyChat(string chat)
	{
		ChatProcess(ChatType.Party, 0L, null, chat, null, isShowBattleEmoti: false);
	}

	public void SendPartyChatShowBattleEmoticon(string chat)
	{
		ChatProcess(ChatType.Party, 0L, null, chat, null, isShowBattleEmoti: true);
	}

	void ChattingPartyComponent.Listener.OnSubmit(string chat)
	{
		SendPartyChat(chat);
	}

	List<ChatInfo> ChattingPartyComponent.Listener.GetPartyChat()
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetChatList(ChatType.Party, 0L);
	}

	void ChattingWhisperComponent.Listener.OnSubmit(long accountId, string userName, string chat)
	{
		ChatProcess(ChatType.Whisper, accountId, userName, chat, null, isShowBattleEmoti: false);
	}

	void ChattingWhisperComponent.Listener.OnAdd()
	{
		AddTask(base.TaskFactory.Run((ChattingWhisperTarget window) => window.OpenAsync(immediately: true)), base.TaskFactory.Run(delegate(ChattingWhisperTarget window)
		{
			window.Init(this);
		}));
		GetInstance<ChattingWhisperTarget>().Open(this);
	}

	void ChattingWhisperComponent.Listener.OnRemove(long accountId, string userName)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Chatting.CHAT_ASK_REMOVE_WHISPER_USER, userName);
		MessagePopup.Open(title, message, delegate
		{
			AddTask(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<ChattingManager>.instance.ClearChatList(ChatType.Whisper, accountId);
			}), base.TaskFactory.Run(delegate(ChattingWindow window)
			{
				window.RemoveUser(accountId);
			}), base.TaskFactory.Run(delegate
			{
				ClearMessageReq data = new ClearMessageReq
				{
					MessageTypeName = typeof(WhisperChat).Name,
					RemoveAccountId = accountId
				};
				GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.Send(data);
			}));
		}, null);
	}

	List<ChatInfo> ChattingWhisperComponent.Listener.GetWhisperChat(long accountId)
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetChatList(ChatType.Whisper, accountId);
	}

	int ChattingWhisperComponent.Listener.GetAlertCount(long accountId)
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetAlertCount(ChatType.Whisper, accountId);
	}

	void ChattingWhisperComponent.Listener.OnSelectUser(long accountId)
	{
		_lastWhisperAccountId = accountId;
		Vespa_Common_Class_Extension_Method.Interface((ChattingWindow.Listener)this).SaveLastChatInfo(ChatType.Whisper, accountId);
	}

	List<Tuple<long, string>> ChattingWhisperComponent.Listener.GetWhisperUser()
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetWhisperUserList();
	}

	void ChattingWhisperComponent.Listener.OnClickGameOption()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OpenGameOptionWindow(GameOptionTab.Game);
		}
	}

	void ChattingWhisperTarget.Listener.OnWhisperTarget(string userName)
	{
		if (string.Equals(userName, GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick, StringComparison.InvariantCultureIgnoreCase))
		{
			MessagePopup.OpenError(ResultType.CannotSearchMyself);
			return;
		}
		if (string.Equals(userName, GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick, StringComparison.InvariantCultureIgnoreCase))
		{
			MessagePopup.OpenError(ResultType.CannotSearchMyself);
			return;
		}
		ChattingWindow window = GetInstance<ChattingWindow>();
		if (window.HasReceiver(userName))
		{
			window.SelectUser(userName);
			return;
		}
		NShared.SearchFriend.Request request = new NShared.SearchFriend.Request();
		request.Keyword = userName;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SearchFriend.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (__Extension_Method_ICollection__.COUNT(response.FriendInfos) > 0)
			{
				long accountId = response.FriendInfos[0].AccountId;
				GlobalAutoBehaviour<ChattingManager>.instance.AddWhisperUser(accountId, userName);
				window.SelectUser(accountId);
			}
		});
	}

	void ItemLinkButtonComponent.Listener.OnSubmit(ChatType chatType, LinkedItemInfo lastLinkedItem, long accountId, string userName)
	{
		if (lastLinkedItem != null)
		{
			ItemNameBuilder.GetItemName(GameRoot.Instance.ItemDataContainer.GetData(lastLinkedItem.EquipItemInfo.ItemIndex), lastLinkedItem.EquipItemInfo);
			string chat = lastLinkedItem.EquipItemInfo.ItemIndex.ToString();
			LinkedItemInfo[] lastLinkedItem2 = new LinkedItemInfo[1] { lastLinkedItem };
			SendChat(chatType, accountId, userName, chat, lastLinkedItem2, isShowBattleEmoti: false);
			RecordLastSendTime(ChatType.Channel);
		}
	}

	bool ChattingEmoticonSelectPopup.Listener.CheckEnableChatting()
	{
		return false;
	}

	Vector3 ChattingEmoticonSelectPopup.Listener.GetRootPosition()
	{
		if (GetEmotionSelectPopupRoot() == null)
		{
			return default(Vector3);
		}
		return GetEmotionSelectPopupRoot().position;
	}

	ChatType ChattingEmoticonSelectPopup.Listener.GetChatType()
	{
		ChattingWindow chattingWindow = GetInstance<ChattingWindow>();
		if (!chattingWindow.IsOpen())
		{
			return ChatType.None;
		}
		return chattingWindow.CurrentChatType;
	}
}
