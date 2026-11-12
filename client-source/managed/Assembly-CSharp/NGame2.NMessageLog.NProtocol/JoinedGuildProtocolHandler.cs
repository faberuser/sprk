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

public class JoinedGuildProtocolHandler : MessageProtocolHandler<JoinedGuild>
{
	public override void Run(long senderId, JoinedGuild protocolData)
	{
		if (protocolData.MemberInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
		{
			return;
		}
		GuildManager guildManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance : null);
		if (!(guildManager == null))
		{
			GuildMemberManager memberManager = guildManager.MemberManager;
			if (memberManager != null && memberManager.Get(senderId) == null)
			{
				guildManager.AddGuildMember(protocolData.MemberInfo);
				memberManager.SetOnline(senderId, isOnline: true);
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
				string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_NEW_MEMBER, protocolData.MemberInfo.Nick);
				GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
			}
		}
	}
}
