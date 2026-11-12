using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NChatting.NUserBlock;
using NGame2.NGuild;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NChatting;

public class ChattingHistory
{
	public class ClientChatInfo
	{
		public ChatInfo ChatInfo;

		public DateTime AddedTime = DateTime.UtcNow;
	}

	public class WhisperInfo
	{
		public long AccountId;

		public string UserName;

		public List<ClientChatInfo> ChatList = new List<ClientChatInfo>();

		public int AlertCount;

		public DateTime CheckedTime = IntialChatCheckedTime;
	}

	public static DateTime IntialChatCheckedTime = new DateTime(2000, 1, 1, 1, 0, 0, DateTimeKind.Utc);

	private List<ClientChatInfo> _channelChatList = new List<ClientChatInfo>();

	private List<ClientChatInfo> _guildChatList = new List<ClientChatInfo>();

	private List<ClientChatInfo> _partyChatList = new List<ClientChatInfo>();

	private Dictionary<long, WhisperInfo> _whisperChatList = new Dictionary<long, WhisperInfo>();

	private DateTime _channelChatCheckedTime = IntialChatCheckedTime;

	private DateTime _guildChatCheckedTime = IntialChatCheckedTime;

	private DateTime _partyChatCheckedTime = IntialChatCheckedTime;

	private int _channelAlertCount;

	private int _guildAlertCount;

	private int _partyAlertCount;

	private List<IChatListener> _listenerList = new List<IChatListener>();

	public void AddListener(IChatListener listener)
	{
		if (!_listenerList.Contains(listener))
		{
			_listenerList.Add(listener);
		}
	}

	public bool RemoveListener(IChatListener listener)
	{
		return _listenerList.Remove(listener);
	}

	public void Clear(ChatType chatType, long accountId)
	{
		if (chatType == ChatType.None || chatType == ChatType.Channel)
		{
			ClearChannelChatHistory();
		}
		if (chatType == ChatType.None || chatType == ChatType.Guild)
		{
			ClearGuildChatHistory();
		}
		if (chatType == ChatType.None || chatType == ChatType.Party)
		{
			ClearPartyChatHistory();
		}
		if (chatType == ChatType.None || chatType == ChatType.Whisper)
		{
			ClearWhisperChatHistory(accountId);
		}
	}

	public void ClearChannelChatHistory()
	{
		_channelChatList.Clear();
		_channelAlertCount = 0;
	}

	public void ClearGuildChatHistory()
	{
		_guildChatList.Clear();
		_guildAlertCount = 0;
	}

	public void ClearPartyChatHistory()
	{
		_partyChatList.Clear();
		_partyAlertCount = 0;
	}

	public void ClearWhisperChatHistory(long accountId)
	{
		if (accountId == 0L)
		{
			foreach (KeyValuePair<long, WhisperInfo> pair in _whisperChatList)
			{
				_listenerList.ForEach(delegate(IChatListener e)
				{
					e.OnWhisperUserRemoved(pair.Key);
				});
			}
			_whisperChatList.Clear();
		}
		else
		{
			_whisperChatList.Remove(accountId);
			_listenerList.ForEach(delegate(IChatListener e)
			{
				e.OnWhisperUserRemoved(accountId);
			});
		}
	}

	public void MakeChatChecked(ChatType chatType, long accountId)
	{
		switch (chatType)
		{
		case ChatType.Channel:
			_channelChatCheckedTime = DateTime.UtcNow;
			_channelAlertCount = 0;
			break;
		case ChatType.Guild:
			_guildChatCheckedTime = DateTime.UtcNow;
			_guildAlertCount = 0;
			break;
		case ChatType.Party:
			_partyChatCheckedTime = DateTime.UtcNow;
			_partyAlertCount = 0;
			break;
		case ChatType.Whisper:
		{
			WhisperInfo value = null;
			if (_whisperChatList.TryGetValue(accountId, out value))
			{
				value.AlertCount = 0;
				value.CheckedTime = DateTime.UtcNow;
			}
			break;
		}
		}
	}

	public int GetAlertCount(ChatType chatType, long accountId)
	{
		switch (chatType)
		{
		case ChatType.Channel:
			return 0;
		case ChatType.Guild:
			return _guildAlertCount;
		case ChatType.Party:
			return _partyAlertCount;
		case ChatType.Whisper:
		{
			int num = 0;
			{
				foreach (KeyValuePair<long, WhisperInfo> whisperChat in _whisperChatList)
				{
					if (accountId == 0L || whisperChat.Key == accountId)
					{
						num += whisperChat.Value.AlertCount;
					}
				}
				return num;
			}
		}
		default:
			return 0;
		}
	}

	public int GetTotalAlertCount(bool refresh)
	{
		if (refresh)
		{
			RefreshAlertCount();
		}
		return 0 + GetAlertCount(ChatType.Channel, 0L) + GetAlertCount(ChatType.Guild, 0L) + GetAlertCount(ChatType.Party, 0L) + GetAlertCount(ChatType.Whisper, 0L);
	}

	public void RefreshAlertCount()
	{
		RefreshChannelChatAlertCount();
		RefreshGuildChatAlertCount();
		RefreshPartyChatAlertCount();
		RefreshWhisperChatAlertCount(0L);
	}

	private void RefreshChannelChatAlertCount()
	{
		_channelAlertCount = GetChatAlertCountInternal(_channelChatList, _channelChatCheckedTime);
	}

	private void RefreshGuildChatAlertCount()
	{
		_guildAlertCount = GetChatAlertCountInternal(_guildChatList, _guildChatCheckedTime);
	}

	private void RefreshPartyChatAlertCount()
	{
		_partyAlertCount = GetChatAlertCountInternal(_partyChatList, _partyChatCheckedTime);
	}

	private void RefreshWhisperChatAlertCount(long whisperAccountId)
	{
		foreach (KeyValuePair<long, WhisperInfo> whisperChat in _whisperChatList)
		{
			_ = whisperChat.Key;
			WhisperInfo value = whisperChat.Value;
			if (whisperAccountId == 0L || whisperChat.Key == whisperAccountId)
			{
				int chatAlertCountInternal = GetChatAlertCountInternal(value.ChatList, value.CheckedTime);
				value.AlertCount = chatAlertCountInternal;
			}
		}
	}

	private int GetChatAlertCountInternal(List<ClientChatInfo> chatInfoList, DateTime checkedTime)
	{
		int num = 0;
		int num2 = __Extension_Method_ICollection__.COUNT(chatInfoList) - 1;
		while (num2 >= 0 && (chatInfoList[num2].AddedTime - checkedTime).TotalSeconds > 0.0)
		{
			num++;
			num2--;
		}
		return num;
	}

	public List<ChatInfo> GetChatList(ChatType chatType, long accountId)
	{
		switch (chatType)
		{
		case ChatType.Channel:
			return _channelChatList.ConvertAll((ClientChatInfo e) => e.ChatInfo);
		case ChatType.Guild:
			return _guildChatList.ConvertAll((ClientChatInfo e) => e.ChatInfo);
		case ChatType.Party:
			return _partyChatList.ConvertAll((ClientChatInfo e) => e.ChatInfo);
		case ChatType.Whisper:
		{
			WhisperInfo value = null;
			if (_whisperChatList.TryGetValue(accountId, out value))
			{
				return value.ChatList.ConvertAll((ClientChatInfo e) => e.ChatInfo);
			}
			break;
		}
		}
		return null;
	}

	public List<Tuple<long, string>> GetWhisperUserList()
	{
		List<Tuple<long, string>> list = new List<Tuple<long, string>>();
		foreach (KeyValuePair<long, WhisperInfo> whisperChat in _whisperChatList)
		{
			list.Add(new Tuple<long, string>(whisperChat.Key, whisperChat.Value.UserName));
		}
		return list;
	}

	public bool AddChat(ChatInfo chatInfo, bool showPreview)
	{
		return AddChat(chatInfo, showPreview ? ChatFlag.ShowPreview : ChatFlag.None);
	}

	public bool AddChat(ChatInfo chatInfo, ChatFlag flag, EmotiType emoticon = EmotiType.None)
	{
		if (chatInfo == null)
		{
			return false;
		}
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if (((chatInfo.EmoticonIndex != 0) ? GameRoot.Instance.EmoticonDataContainer.GetData(chatInfo.EmoticonIndex) : null) == null && String_Extension_Method.IsNullOrEmpty(chatInfo.Chat))
		{
			return false;
		}
		long num = chatInfo.SenderId;
		bool flag2 = num == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		bool showPreview = (flag & ChatFlag.ShowPreview) != 0;
		if (!flag2 && SingletonAuto<UserBlockManager>.instance.IsBlockedUser(num))
		{
			return false;
		}
		if (!flag2 && !SystemOptionData.Chat_AllowAllWhisper && chatInfo.ChatType == ChatType.Whisper)
		{
			return false;
		}
		if (!flag2 && SystemOptionData.Chat_AllowAllWhisper && chatInfo.ChatType == ChatType.Whisper)
		{
			bool flag3 = false;
			bool chat_AllowFriendWhisper = SystemOptionData.Chat_AllowFriendWhisper;
			bool num2 = GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsFriend(num);
			if (num2 && !chat_AllowFriendWhisper)
			{
				flag3 = (byte)((flag3 ? 1u : 0u) | 1u) != 0;
			}
			bool chat_AllowGuildWhisper = SystemOptionData.Chat_AllowGuildWhisper;
			bool flag4 = GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null && GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Get(num) != null;
			if (flag4 && !chat_AllowGuildWhisper)
			{
				flag3 = (byte)((flag3 ? 1u : 0u) | 1u) != 0;
			}
			bool chat_AllowEtcWhisper = SystemOptionData.Chat_AllowEtcWhisper;
			if (!num2 && !flag4 && !chat_AllowEtcWhisper)
			{
				flag3 = (byte)((flag3 ? 1u : 0u) | 1u) != 0;
			}
			if (flag3)
			{
				return false;
			}
		}
		ClientChatInfo clientChatInfo = new ClientChatInfo
		{
			ChatInfo = chatInfo,
			AddedTime = DateTime.UtcNow
		};
		switch (chatInfo.ChatType)
		{
		case ChatType.Channel:
		{
			if (_channelChatList.Count > 0 && !chatInfo.IsSystem)
			{
				ClientChatInfo clientChatInfo2 = _channelChatList[_channelChatList.Count - 1];
				if (clientChatInfo2 != null && clientChatInfo2.ChatInfo != null && clientChatInfo2.ChatInfo.IsSameChat(chatInfo))
				{
					return false;
				}
			}
			_channelChatList.Add(clientChatInfo);
			int i = 0;
			int maxChatCount3 = ChattingManager.MaxChatCount;
			while (_channelChatList.Count > maxChatCount3)
			{
				for (; _channelChatList[i].ChatInfo.isAlwaysShow; i++)
				{
				}
				_channelChatList.RemoveAt(i);
			}
			break;
		}
		case ChatType.Guild:
		{
			if (IsDuplicatedChat(clientChatInfo, _guildChatList))
			{
				return false;
			}
			_guildChatList.Add(clientChatInfo);
			int maxChatCount4 = ChattingManager.MaxChatCount;
			while (_guildChatList.Count > maxChatCount4)
			{
				_guildChatList.RemoveAt(0);
			}
			break;
		}
		case ChatType.Party:
		{
			_partyChatList.Add(clientChatInfo);
			int maxChatCount2 = ChattingManager.MaxChatCount;
			while (_partyChatList.Count > maxChatCount2)
			{
				_partyChatList.RemoveAt(0);
			}
			break;
		}
		case ChatType.Whisper:
		{
			string userName = chatInfo.SenderName;
			if (num == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				num = chatInfo.ReceiverId;
				userName = chatInfo.ReceiverName;
			}
			WhisperInfo value = null;
			if (!_whisperChatList.TryGetValue(num, out value))
			{
				value = AddWhisperUser(num, userName);
			}
			if (IsDuplicatedChat(clientChatInfo, value.ChatList))
			{
				return false;
			}
			value.ChatList.Add(clientChatInfo);
			int maxChatCount = ChattingManager.MaxChatCount;
			while (value.ChatList.Count > maxChatCount)
			{
				value.ChatList.RemoveAt(0);
			}
			break;
		}
		}
		_listenerList.ForEach(delegate(IChatListener e)
		{
			e.OnChatAdded(chatInfo, showPreview, emoticon);
		});
		return true;
	}

	private static bool IsDuplicatedChat(ClientChatInfo newChat, List<ClientChatInfo> chatList)
	{
		if (__Extension_Method_ICollection__.COUNT(chatList) == 0)
		{
			return false;
		}
		ClientChatInfo clientChatInfo = chatList[chatList.Count - 1];
		if (clientChatInfo != null && clientChatInfo.ChatInfo != null && newChat.ChatInfo != null)
		{
			if (clientChatInfo.AddedTime > newChat.AddedTime)
			{
				return true;
			}
			if (clientChatInfo.AddedTime == newChat.AddedTime && clientChatInfo.ChatInfo.SenderId == newChat.ChatInfo.SenderId && clientChatInfo.ChatInfo.Chat == newChat.ChatInfo.Chat)
			{
				return true;
			}
		}
		return false;
	}

	public WhisperInfo AddWhisperUser(long accountId, string userName)
	{
		if (_whisperChatList.ContainsKey(accountId))
		{
			return _whisperChatList[accountId];
		}
		WhisperInfo whisperInfo = new WhisperInfo();
		whisperInfo.AccountId = accountId;
		whisperInfo.UserName = userName;
		_whisperChatList.Add(accountId, whisperInfo);
		_listenerList.ForEach(delegate(IChatListener e)
		{
			e.OnWhisperUserAdded(accountId, userName);
		});
		return whisperInfo;
	}
}
