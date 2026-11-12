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

public class AcceptJoinGuildProtocolHandler : MessageProtocolHandler<AcceptJoinGuild>
{
	public override void Run(long senderId, AcceptJoinGuild protocolData)
	{
		if (protocolData.MemberInfo.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
		{
			GlobalAutoBehaviour<GuildManager>.instance.AddGuildMember(protocolData.MemberInfo);
			GlobalAutoBehaviour<GuildManager>.instance.RemoveJoinRequestPlayer(protocolData.MemberInfo.AccountId);
			SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.Waiting);
			string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_NEW_MEMBER, protocolData.MemberInfo.Nick);
			GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
		}
	}
}
