using NGame2.NChatting;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class ChatBanProtocolHandler : MessageProtocolHandler<ChatBan>
{
	public override void Run(long senderId, ChatBan protocolData)
	{
		if (protocolData == null || protocolData.Infos == null)
		{
			return;
		}
		bool flag = false;
		ChatBanInfo[] infos = protocolData.Infos;
		foreach (ChatBanInfo chatBanInfo in infos)
		{
			if (chatBanInfo.BanReason == ChatBanReason.ReleaseBan)
			{
				GlobalAutoBehaviour<ChattingManager>.instance.RemoveChatBanInfo(chatBanInfo.BanType);
				continue;
			}
			GlobalAutoBehaviour<ChattingManager>.instance.AddChatBanInfo(chatBanInfo);
			flag = true;
		}
		if (flag)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("CHATTING_BAN_CENTER_MESSAGE", protocolData.BanHour));
		}
	}
}
