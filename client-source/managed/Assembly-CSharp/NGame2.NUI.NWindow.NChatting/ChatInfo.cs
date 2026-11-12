using System;
using NGame2.NChatting;
using NShared;
using NShared.NMessage.NProtocol;

namespace NGame2.NUI.NWindow.NChatting;

public class ChatInfo
{
	public ChatType ChatType;

	public long SenderId;

	public string SenderName;

	public long ReceiverId;

	public string ReceiverName;

	public int SenderAvaterIndex;

	public int SenderTeamLevel;

	public string Chat;

	public bool IsSystem;

	public bool isAlwaysShow;

	public int AdminLevel;

	public string SendTime;

	public LinkedItemInfo[] LinkedItem;

	public int EmoticonIndex;

	public bool IsShowBattleEmoti;

	public int[] ChatIconList;

	public ChattingActionData ActionData;

	public IChattingAction Action;

	public Action<ChatInfo> OnClick;

	public bool IsSameChat(ChatInfo other)
	{
		if (other.SenderId == SenderId && other.Chat == Chat)
		{
			return other.EmoticonIndex == EmoticonIndex;
		}
		return false;
	}
}
