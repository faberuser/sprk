using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NConsole;
using NGame2.NMessageLog;
using NGame2.NParty;
using NGame2.NSocket;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NShared.NLocalization;
using NShared.NMessage;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NChatting;

public class ChattingManager : GlobalAutoBehaviour<ChattingManager>
{
	private ChattingActionContainer _actionContainer;

	private ChattingHistory _chattingHistory;

	private float _lastRefreshAlertCountTime;

	private List<IChannelListener> _channelListenerList = new List<IChannelListener>();

	private bool _recordedChatAdded;

	private bool _isChatCautionChatting;

	public bool IsShowGuildNotice;

	private Dictionary<ChatBanType, ChatBanInfo> ChatBanInfoList = new Dictionary<ChatBanType, ChatBanInfo>();

	public static int MaxChatCount => GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxChatCount");

	protected override void Awake()
	{
		base.Awake();
		_lastRefreshAlertCountTime = Time.realtimeSinceStartup;
		_actionContainer = new ChattingActionContainer();
		_chattingHistory = new ChattingHistory();
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<ChannelChat>
		{
			Handler = delegate(long s, ChannelChat d)
			{
				AddChannelChat(s, d, ChatFlag.ShowPreview);
			}
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<GuildChat>
		{
			Handler = delegate(long s, GuildChat d)
			{
				AddGuildChat(s, d, ChatFlag.ShowPreview);
			}
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<WhisperChat>
		{
			Handler = delegate(long s, WhisperChat d)
			{
				AddWhisperChat(s, d, ChatFlag.ShowPreview);
			}
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyChat>
		{
			Handler = delegate(long s, PartyChat d)
			{
				AddPartyChat(s, d, ChatFlag.ShowPreview);
			}
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<WorldChat>
		{
			Handler = delegate(long s, WorldChat d)
			{
				AddWorldChat(s, d, ChatFlag.ShowPreview);
			}
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyMemberEmoti>
		{
			Handler = AddEmoti
		});
	}

	public void RegisterAction(IChattingAction action)
	{
		_actionContainer.Register(action);
	}

	private void Update()
	{
		UpdateRefreshAlertCount();
	}

	private void UpdateRefreshAlertCount()
	{
		if (!(Time.realtimeSinceStartup - _lastRefreshAlertCountTime < 1000f))
		{
			_lastRefreshAlertCountTime = Time.realtimeSinceStartup;
			_chattingHistory.RefreshAlertCount();
		}
	}

	public void AddChannelListener(IChannelListener listener)
	{
		if (!_channelListenerList.Contains(listener))
		{
			_channelListenerList.Add(listener);
		}
	}

	public void RemoveChannelListener(IChannelListener listener)
	{
		_channelListenerList.Remove(listener);
	}

	public void AddChatListener(IChatListener listener)
	{
		_chattingHistory.AddListener(listener);
	}

	public void RemoveChatListener(IChatListener listener)
	{
		_chattingHistory.RemoveListener(listener);
	}

	public void MakeChatChecked(ChatType chatType, long accountId)
	{
		_chattingHistory.MakeChatChecked(chatType, accountId);
	}

	public int GetTotalAlertCount(bool refresh)
	{
		return _chattingHistory.GetTotalAlertCount(refresh);
	}

	public int GetAlertCount(ChatType chatType, long accountId = 0L)
	{
		return _chattingHistory.GetAlertCount(chatType, accountId);
	}

	public void ClearChatList(ChatType chatType = ChatType.None, long accountId = 0L)
	{
		_chattingHistory.Clear(chatType, accountId);
	}

	public List<ChatInfo> GetChatList(ChatType chatType, long accountId = 0L)
	{
		return _chattingHistory.GetChatList(chatType, accountId);
	}

	public List<Tuple<long, string>> GetWhisperUserList()
	{
		return _chattingHistory.GetWhisperUserList();
	}

	public void AddWhisperUser(long accountId, string userName)
	{
		_chattingHistory.AddWhisperUser(accountId, userName);
	}

	public void AddChannelChat(long senderId, ChannelChat data, ChatFlag flag, Action<ChatInfo> onClick = null)
	{
		ChatInfo chatInfo = CreateReceivedChatInfo(senderId, ChatType.Channel, data, onClick);
		_chattingHistory.AddChat(chatInfo, ChatFlag.ShowPreview);
	}

	public void AddGuildChat(long senderId, GuildChat data, ChatFlag flag, Action<ChatInfo> onClick = null)
	{
		if (GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.GuildId == 0)
		{
			return;
		}
		ChatInfo chatInfo = CreateReceivedChatInfo(senderId, ChatType.Guild, data, onClick);
		chatInfo.IsSystem = senderId == 0;
		if (chatInfo.IsSystem)
		{
			string[] array = chatInfo.Chat.Split(',');
			if (array.Length < 2)
			{
				return;
			}
			string key = array[0].Remove(0, 1);
			if (data.IsGuildNotice)
			{
				int num = chatInfo.Chat.IndexOf(",");
				string text = chatInfo.Chat.Substring(num + 1);
				chatInfo.Chat = LocalizationManager.GetStringDirect(key, text);
			}
			else
			{
				List<string> list = array.ToList();
				list.RemoveAt(0);
				object[] args = list.ToArray();
				chatInfo.Chat = LocalizationManager.GetStringDirect(key, args);
			}
		}
		_chattingHistory.AddChat(chatInfo, flag);
	}

	public void AddPartyChat(long senderId, PartyChat data, ChatFlag flag, Action<ChatInfo> onClick = null)
	{
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo() != null && GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(senderId) != null)
		{
			ChatInfo chatInfo = CreateReceivedChatInfo(senderId, ChatType.Party, data, onClick);
			_chattingHistory.AddChat(chatInfo, flag);
		}
	}

	public void AddWhisperChat(long senderId, WhisperChat data, ChatFlag flag, Action<ChatInfo> onClick = null)
	{
		ChatInfo chatInfo = CreateReceivedChatInfo(senderId, ChatType.Whisper, data, onClick);
		_chattingHistory.AddChat(chatInfo, flag);
	}

	public void AddWorldChat(long senderId, WorldChat data, ChatFlag flag, Action<ChatInfo> onClick = null)
	{
		ChatInfo chatInfo = CreateReceivedChatInfo(senderId, ChatType.World, data, onClick);
		chatInfo.IsSystem = true;
		_chattingHistory.AddChat(chatInfo, flag);
	}

	public void AddEmoti(long senderId, PartyMemberEmoti data)
	{
		string stringDirect = LocalizationManager.GetStringDirect("EMOTI_" + data.Emoti.ToString().ToUpper(), data.SenderName, data.ReceiverName);
		if (!String_Extension_Method.IsNullOrEmpty(stringDirect))
		{
			ChatInfo chatInfo = new ChatInfo();
			chatInfo.ChatType = data.ChatType;
			chatInfo.SenderId = senderId;
			chatInfo.SenderName = data.SenderName;
			chatInfo.SenderAvaterIndex = data.SenderAvatarIndex;
			chatInfo.SenderTeamLevel = data.SenderTeamLevel;
			chatInfo.ReceiverId = data.ReceiverId;
			chatInfo.ReceiverName = data.ReceiverName;
			chatInfo.Chat = stringDirect;
			chatInfo.IsSystem = true;
			_chattingHistory.AddChat(chatInfo, ChatFlag.ShowPreview, data.Emoti);
		}
	}

	public void AddCautionSystemChat()
	{
		if (IAPWrapper.StoreType == IAPWrapper.EStoreType.OneStore && GlobalAutoBehaviour<AccountManager>.instance.CountryServerName.Equals("Korea") && SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.Korean && !_isChatCautionChatting)
		{
			AddSystemChat("비방/욕설/음란/광고 등 불건전 행위는 운영 정책에 의거 제재 대상이 되며, 피해가 발생 할 수 있으므로 결제정보 및 개인정보는 절대 타인에게 공개하지 마시기 바랍니다.", ChatType.Channel, showPreview: true, 0L, alwaysShow: true);
			_isChatCautionChatting = true;
		}
	}

	public void AddSystemChat(string chat, ChatType chatType = ChatType.Channel, bool showPreview = true, long receiverID = 0L, bool alwaysShow = false)
	{
		if (!string.IsNullOrEmpty(chat))
		{
			ChatInfo chatInfo = new ChatInfo();
			chatInfo.IsSystem = true;
			chatInfo.ChatType = chatType;
			chatInfo.Chat = chat;
			chatInfo.isAlwaysShow = alwaysShow;
			if (chatType == ChatType.Whisper)
			{
				chatInfo.SenderId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
				chatInfo.ReceiverId = receiverID;
			}
			_chattingHistory.AddChat(chatInfo, showPreview);
		}
	}

	public void AddGuildSystemChat(string chat, bool showPreview = true)
	{
		if (!string.IsNullOrEmpty(chat))
		{
			ChatInfo chatInfo = new ChatInfo();
			chatInfo.IsSystem = true;
			chatInfo.ChatType = ChatType.Guild;
			chatInfo.Chat = chat;
			chatInfo.SendTime = NGame2.NUI.NWindow.Common.GetUTCNow().ToString(Vespa_DateTime_Extension_Method.NetworkFormat2);
			_chattingHistory.AddChat(chatInfo, showPreview);
		}
	}

	public string SerializeChattingActionData<T>(T subData) where T : class
	{
		string data = JsonMarshaler.EncodeJsonString(subData);
		string text = typeof(T).Name;
		return JsonMarshaler.EncodeJsonString(new ChattingActionData
		{
			Name = text,
			Data = data
		});
	}

	private ChattingActionData CreateActionData(string message)
	{
		if (String_Extension_Method.IsNullOrEmpty(message))
		{
			return null;
		}
		if (!message.StartsWith("{") || !message.EndsWith("}"))
		{
			return null;
		}
		return JsonMarshaler.Parse<ChattingActionData>(message);
	}

	public ChatInfo CreateReceivedChatInfo<T>(long senderId, ChatType chatType, T data, Action<ChatInfo> onClick = null) where T : BaseChat
	{
		ChatInfo chatInfo = new ChatInfo();
		chatInfo.ChatType = chatType;
		chatInfo.SenderId = senderId;
		chatInfo.SenderName = data.SenderName;
		chatInfo.ReceiverId = data.ReceiverId;
		chatInfo.ReceiverName = data.ReceiverName;
		chatInfo.SenderAvaterIndex = data.SenderAvatarIndex;
		chatInfo.SenderTeamLevel = data.SenderTeamLevel;
		chatInfo.SendTime = data.SendTime;
		chatInfo.OnClick = onClick;
		chatInfo.Chat = data.Chat;
		chatInfo.AdminLevel = data.AdminLevel;
		chatInfo.LinkedItem = data.LinkedItem;
		chatInfo.EmoticonIndex = data.EmoticonIndex;
		chatInfo.IsShowBattleEmoti = data.IsShowBattleEmoti;
		chatInfo.ChatIconList = data.ChatIconList;
		if (chatType == ChatType.World && senderId == 0L)
		{
			chatInfo.Chat = LocalizationManager.LocalizeCode(data.Chat);
		}
		ChattingActionData chattingActionData = CreateActionData(chatInfo.Chat);
		if (chattingActionData != null)
		{
			chattingActionData.SenderId = senderId;
			chattingActionData.SenderName = data.SenderName;
			chattingActionData.ReceiverId = data.ReceiverId;
			chattingActionData.ReceiverName = data.ReceiverName;
			chatInfo.ActionData = chattingActionData;
			chatInfo.Action = _actionContainer.GetAction(chattingActionData.Name);
			if (chatInfo.Action != null)
			{
				chatInfo.Chat = chatInfo.Action.ConvertChat(chattingActionData);
			}
		}
		return chatInfo;
	}

	public bool ProcessCheatCommand(ChatType chatType, string chat)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.AdminLevel == 0)
		{
			return false;
		}
		if (string.IsNullOrEmpty(chat))
		{
			return false;
		}
		if (!chat.StartsWith("/"))
		{
			return false;
		}
		string text = chat.Substring(1);
		bool isHelp = string.Equals(text, "HELP", StringComparison.InvariantCultureIgnoreCase);
		SingletonAuto<ConsoleInputManager>.instance.OnConsoleInput(text, delegate(string responseMessage)
		{
			if (!string.IsNullOrEmpty(responseMessage))
			{
				string[] collection = responseMessage.Split(new char[1] { '\n' });
				List<string> list = new List<string>();
				list.AddRange(collection);
				list.RemoveAll(string.IsNullOrEmpty);
				list = list.Distinct().ToList();
				if (isHelp)
				{
					list.Sort();
				}
				ChatInfo chatInfo = new ChatInfo
				{
					Chat = "CHEAT : " + Vespa_String_Extension_Method.NguiColor(chat, Color.red),
					ChatType = chatType,
					IsSystem = true
				};
				_chattingHistory.AddChat(chatInfo, showPreview: false);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
				{
					ChatInfo chatInfo2 = new ChatInfo
					{
						Chat = " >> " + list[i],
						ChatType = chatType,
						IsSystem = true
					};
					_chattingHistory.AddChat(chatInfo2, showPreview: false);
				}
			}
		});
		return true;
	}

	public void OnChattingServerConnected(LoginRes loginRes)
	{
		if (_recordedChatAdded)
		{
			return;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(loginRes.GuildChats))
		{
			MessageNot[] guildChats = loginRes.GuildChats;
			foreach (MessageNot obj in guildChats)
			{
				long senderId = obj.SenderId;
				GuildChat data = JsonMarshaler.Parse<GuildChat>(obj.Content);
				AddGuildChat(senderId, data, ChatFlag.Recorded);
			}
			_chattingHistory.MakeChatChecked(ChatType.Guild, 0L);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(loginRes.WhisperChats))
		{
			MessageNot[] guildChats = loginRes.WhisperChats;
			foreach (MessageNot obj2 in guildChats)
			{
				long senderId2 = obj2.SenderId;
				long[] receiverIds = obj2.ReceiverIds;
				WhisperChat data2 = JsonMarshaler.Parse<WhisperChat>(obj2.Content);
				AddWhisperChat(senderId2, data2, ChatFlag.Recorded);
				_chattingHistory.MakeChatChecked(ChatType.Whisper, senderId2);
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(receiverIds); j++)
				{
					_chattingHistory.MakeChatChecked(ChatType.Whisper, receiverIds[j]);
				}
			}
		}
		_recordedChatAdded = true;
	}

	public void OnChattingServerDisonnected()
	{
		OnChannelLeave();
	}

	public void OnChannelEnter(int channelNo)
	{
		string chat = LocalizationManager.GetString(Chatting.CHANNEL_CHATTING_CONNECTED, channelNo);
		AddSystemChat(chat, ChatType.Channel, showPreview: true, 0L);
		_channelListenerList.ForEach(delegate(IChannelListener e)
		{
			e.OnChannelEnter(channelNo);
		});
	}

	public void OnChannelLeave()
	{
		string chat = LocalizationManager.GetString(Chatting.CHANNEL_CHATTING_DISCONNECTED);
		AddSystemChat(chat, ChatType.Channel, showPreview: true, 0L);
		_channelListenerList.ForEach(delegate(IChannelListener e)
		{
			e.OnChannelLeave();
		});
	}

	public bool IsAnyChatBan()
	{
		if (ChatBanInfoList.Count() == 0)
		{
			return false;
		}
		foreach (ChatBanInfo value in ChatBanInfoList.Values)
		{
			if (IsChatBanTime(value))
			{
				return true;
			}
		}
		return false;
	}

	public static ChatBanType GetChatBanType(ChatType chatType)
	{
		ChatBanType result = ChatBanType.All;
		switch (chatType)
		{
		case ChatType.Channel:
			result = ChatBanType.Global;
			break;
		case ChatType.Guild:
			result = ChatBanType.Guild;
			break;
		case ChatType.Party:
			result = ChatBanType.Party;
			break;
		case ChatType.Whisper:
			result = ChatBanType.Whisper;
			break;
		}
		return result;
	}

	public bool IsChatBan(ChatType chatType)
	{
		if (!IsAnyChatBan())
		{
			return false;
		}
		ChatBanType chatBanType = GetChatBanType(chatType);
		ChatBanInfo chatBanInfo = GetChatBanInfo(chatBanType);
		if (chatBanInfo == null)
		{
			return false;
		}
		return IsChatBanTime(chatBanInfo);
	}

	public ChatBanInfo GetChatBanInfo(ChatBanType banType)
	{
		if (!ChatBanInfoList.ContainsKey(banType))
		{
			return null;
		}
		return ChatBanInfoList[banType];
	}

	public void SetChatBanInfos(ChatBanInfo[] infos)
	{
		if (infos != null && ChatBanInfoList != null)
		{
			ChatBanInfoList.Clear();
			foreach (ChatBanInfo info in infos)
			{
				AddChatBanInfo(info);
			}
		}
	}

	public void AddChatBanInfo(ChatBanInfo info)
	{
		if (ChatBanInfoList.ContainsKey(info.BanType))
		{
			ChatBanInfoList[info.BanType] = info;
		}
		else
		{
			ChatBanInfoList.Add(info.BanType, info);
		}
	}

	public void RemoveChatBanInfo(ChatBanType banType)
	{
		ChatBanInfoList.Remove(banType);
	}

	public static bool IsChatBanTime(ChatBanInfo info)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(info.EndTime, out var o_dateTime))
		{
			return false;
		}
		if (NGame2.NUI.NWindow.Common.GetUTCNow() <= o_dateTime)
		{
			return true;
		}
		return false;
	}

	public TimeSpan GetRemainTime(ChatBanType banType)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(GetChatBanInfo(banType).EndTime, out var o_dateTime))
		{
			return TimeSpan.Zero;
		}
		return o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow();
	}

	public string GetChatBanErrorString(ChatBanType banType)
	{
		TimeSpan timeSpan = GetRemainTime(banType).Add(TimeSpan.FromMinutes(1.0));
		string stringDirect = LocalizationManager.GetStringDirect("CHATTING_BAN_MESSAGE", (int)timeSpan.TotalHours, timeSpan.Minutes.ToString("D2"));
		return $"[c][ff0000]{stringDirect}[-][/c]";
	}
}
