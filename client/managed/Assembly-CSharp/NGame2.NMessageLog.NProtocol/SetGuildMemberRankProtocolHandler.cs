using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class SetGuildMemberRankProtocolHandler : MessageProtocolHandler<SetGuildMemberRank>
{
	public override void Run(long senderId, SetGuildMemberRank protocolData)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.SetRank(protocolData.AccountId, (EGuildMemberRank)protocolData.Rank);
			if (protocolData.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
			}
			SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			string text = null;
			switch ((EGuildMemberRank)protocolData.Rank)
			{
			case EGuildMemberRank.GuildNewMember:
				text = LocalizationManager.GetString(Guild.GUILD_RANK_NEW_MEMBER);
				break;
			case EGuildMemberRank.GuildMember:
				text = LocalizationManager.GetString(Guild.GUILD_RANK_MEMBER);
				break;
			case EGuildMemberRank.GuildAdmin:
				text = LocalizationManager.GetString(Guild.GUILD_RANK_ADMIN);
				break;
			case EGuildMemberRank.GuildMaster:
				text = LocalizationManager.GetString(Guild.GUILD_RANK_MASTER);
				break;
			case EGuildMemberRank.GuildGhostMember:
				text = LocalizationManager.GetString(Guild.GUILD_RANK_GHOST_MEMBER);
				break;
			}
			string nick = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Get(protocolData.AccountId).Nick;
			string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_CHANGE_RANK, nick, text);
			GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
		}
	}
}
