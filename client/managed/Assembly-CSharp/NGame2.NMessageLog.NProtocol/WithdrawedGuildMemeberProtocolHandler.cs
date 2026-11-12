using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class WithdrawedGuildMemeberProtocolHandler : MessageProtocolHandler<WithdrawedGuildMember>
{
	public override void Run(long senderId, WithdrawedGuildMember protocolData)
	{
		if (protocolData.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Remove(protocolData.AccountId);
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			}
			string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_WITHDRAW, protocolData.Nick);
			GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
		}
	}
}
